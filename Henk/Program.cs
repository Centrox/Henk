using Henk.ExchangeWebServices;
using System;
using System.Linq;
using System.Net;

namespace Henk
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      string username, password, folder, to;

      Console.WriteLine();
      Console.WriteLine("  COE            NSOFT         WAR        EBV");
      Console.WriteLine("  COE            NSOFTW        ARE       BVC");
      Console.WriteLine("  OEN            SOF TWA       REB     VCO");
      Console.WriteLine("  ENS            OFT  WAR      EBV    COE");
      Console.WriteLine("  NSO            FTW   ARE     BVC   OEN");
      Console.WriteLine("  COENSOFTWAREBV2018    COE    NSO FTW ARE");
      Console.WriteLine("  COENSOFTWAREBV2018     COE   NSOFTW   ARE");
      Console.WriteLine("  BVC            OEN      SOF  TWARE     BVC");
      Console.WriteLine("  OEN            SOF       TWA REBV       COE");
      Console.WriteLine("  NSO            FTW        AREBVC         OEN");
      Console.WriteLine("  SOF            TWA         REBVC          OEN");
      Console.WriteLine("  SOF            TWA          REBV           COEN\r\n");

      #region Get credentials

      if (args.Length == 4 && ((username = args[0]) + (password = args[1]) + (folder = args[2]) + (to = args[3])).Length > 0)
      {
        Console.WriteLine("  Using parameters from command line.");
      }
      else
      {
        string Prompt(string q)
        {
          Console.Write("  " + q + ": ");
          return Console.ReadLine();
        }

        username = Prompt("Exchange username");
        password = Prompt("Exchange password");
        folder = Prompt("Processed folder");
        to = Prompt("Forward address");
      }

      #endregion

      try
      {
        Process(username, password, folder, to);
      }
      catch (WebException ex)
      {
        Console.WriteLine("\r\n  Something went wrong when contacting Exchange - are your credentials correct?\r\n");
        Console.WriteLine(ex);
      }
      catch (Exception ex)
      {
        Console.WriteLine("\r\n  An error occurred, please try again later.\r\n");
        Console.WriteLine(ex);
      }
      finally
      {
        Console.WriteLine("\r\n  <Press Enter to exit>");
        Console.ReadLine();
      }
    }

    private static void Process(string username, string password, string folder, string to)
    {
      Console.WriteLine("\r\n  Processing...\r\n");
      Console.WriteLine("  > Connecting...");

      var binding = new ExchangeServiceBinding
      {
        Url = "https://amxprd0510.outlook.com/ews/exchange.asmx",
        Credentials = new NetworkCredential(username, password),
        RequestServerVersionValue = new RequestServerVersion {Version = ExchangeVersionType.Exchange2010}
      };

      #region Get folder

      Console.WriteLine("  > Retrieving folder...");

      var folderId = binding.FindFolder(new FindFolderType
                            {
                              Traversal = FolderQueryTraversalType.Deep,
                              FolderShape = new FolderResponseShapeType {BaseShape = DefaultShapeNamesType.IdOnly},
                              ParentFolderIds = new BaseFolderIdType[] {new DistinguishedFolderIdType {Id = DistinguishedFolderIdNameType.root}},
                              Restriction = new RestrictionType
                              {
                                Item = new ContainsExpressionType
                                {
                                  ContainmentMode = ContainmentModeType.Substring,
                                  ContainmentModeSpecified = true,
                                  ContainmentComparison = ContainmentComparisonType.IgnoreCase,
                                  ContainmentComparisonSpecified = true,
                                  Item = new PathToUnindexedFieldType {FieldURI = UnindexedFieldURIType.folderDisplayName},
                                  Constant = new ConstantValueType {Value = folder}
                                }
                              }
                            })
                            .ResponseMessages.Items.OfType<FindFolderResponseMessageType>()
                            .First().RootFolder.Folders.First().FolderId;

      #endregion

      #region Get items

      Console.WriteLine("  > Retrieving items...");

      var itemIds = binding.FindItem(new FindItemType
                           {
                             Traversal = ItemQueryTraversalType.Shallow,
                             ItemShape = new ItemResponseShapeType {BaseShape = DefaultShapeNamesType.Default},
                             ParentFolderIds = new BaseFolderIdType[] {new DistinguishedFolderIdType {Id = DistinguishedFolderIdNameType.inbox}}
                           })
                           .ResponseMessages.Items.Select(x => x as FindItemResponseMessageType).Where(x => x != null)
                           .Where(x => x.RootFolder != null && x.RootFolder.TotalItemsInView > 0)
                           .SelectMany(item => ((ArrayOfRealItemsType) item.RootFolder.Item).Items.Select(y => y.ItemId))
                           .ToArray();

      var messages = binding.GetItem(new GetItemType {ItemShape = new ItemResponseShapeType {BaseShape = DefaultShapeNamesType.AllProperties}, ItemIds = itemIds})
                            .ResponseMessages.Items.Select(x => x as ItemInfoResponseMessageType).Where(x => x != null)
                            .Select(x => x.Items.Items[0] as MessageType).Where(x => x != null && x.IsFromMe)
                            .ToList();

      #endregion

      #region Process items

      if (!messages.Any())
      {
        Console.WriteLine("  > No messages to process!");
        return;
      }

      Console.WriteLine("  > Processing " + messages.Count + " messages...\r\n");

      foreach (var message in messages)
      {
        Console.WriteLine("    " + message.Subject);

        // forward message
        binding.CreateItem(new CreateItemType
        {
          MessageDisposition = MessageDispositionType.SendAndSaveCopy,
          MessageDispositionSpecified = true,
          SavedItemFolderId = new TargetFolderIdType {Item = new DistinguishedFolderIdType {Id = DistinguishedFolderIdNameType.sentitems}},
          Items = new NonEmptyArrayOfAllItemsType
          {
            Items = new ItemType[]
            {
              new ForwardItemType
              {
                Subject = "VERK",
                ToRecipients = new[] {new EmailAddressType {EmailAddress = to}},
                ReferenceItemId = new ItemIdType {ChangeKey = message.ItemId.ChangeKey, Id = message.ItemId.Id},
                NewBodyContent = new BodyType {BodyType1 = BodyTypeType.HTML, Value = "Email is automatically forwarded by HENK."}
              }
            }
          }
        });

        // move to subfolder
        binding.MoveItem(new MoveItemType
        {
          ToFolderId = new TargetFolderIdType {Item = folderId},
          ItemIds = new BaseItemIdType[] {message.ItemId}
        });
      }

      Console.WriteLine("\r\n  Finished!");

      #endregion
    }
  }
}