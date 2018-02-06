# Henk

![HNK](/Henk/icon02.png)

Henk forwards invoice emails for Tomas. In his spare time he likes to chew bubblegum.

[![Download](http://apk.altervista.org/wp-content/uploads/2016/12/download-button-free-png-file.png)](https://github.com/coenvdwel/Henk/raw/master/Henk.exe)

## Definition

Henk will look in the supplied Exchange account's Inbox for any email that has been sent by itself, to itself. It just finds the lonely conversions, so to say. This email will be forwarded to the supplied forwarder emailadress with the fixed subject "[VERK]", and then moved to the supplied folder for achiving.

## Start

You can just run the application, in which case Henk will ask for the following things;

* Username: your Exchange username
* Password: your Exchange password (it will not be saved anywhere, but will be visible as you type!)
* Folder: the folder to which to move processed items to
* Forward: the emailadress to forward the processed items to

### Command line parameters

If you find yourself running this more often, you maybe want to pass these 4 parameters as "command line parameters" in a shortcut to this application. If you're unfamiliar, Google is your friend.

1) Click and drag Henk.exe with your right mouse key to a folder (or your Desktop) > choose "Create shortcuts here"
2) Rightclick the newly generated "Henk.exe - Shortcut" file > choose "Properties"
3) In "Target", add the 4 parameters separated by spaces after the existing content

Running this new shortcut will now automatically fill in the parameters and they will be hidden from sight. Remember that your newly created shortcut file contains your username and password for your email, so keep it safe! You do not have to repeat this process if you update Henk.exe with a new version, as long as you do not move it's location on your disk.

### Windows service

One step beyond; you can create a Windows service (maybe even on a Diract Azure machine) to schedule Henk to check your inbox on a set interval, for instance every hour. Henk is perfectly fine being used as cleaning lady!
