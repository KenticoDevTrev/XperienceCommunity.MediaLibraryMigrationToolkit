# XperienceCommunity.MediaLibraryMigrationToolkit
Helps migrate media library files off of database/on premise file storage and onto cloud storage (azure blob storage).

# STATUS
This project is still in processes.  The code files should be up to date, and this has been successfully used on a project, the UI for it has not been fully fleshed out. 

If you need this functionality before i can finish things up, please contact me at tfayas at gmail.

# How it works
There are two primary operations this tool performs:

Firstly, it moves files off the file system into an azure blob storage by 'deleting' the media library items, and re-inserting them (with the same guid).  Once you have Azure Blob Storage configured, this delete and insert operation will upload the media file into the blob storage.

Secondly, since using blob storage requires the use of the /getmedia and/or /getattachment, it scans the database fields (defined in a provided UI for the module), and coupled with a dictionary of possible lookup values (including relative media paths), it finds and replaces any relative media path with the /getmedia path. 

Thirdly, if you wish to move away from attachments, this tool can also replace attachments with the media files.  The GUIDs for the attachment will be used for the media GUID.  This operation however may require some recoding if you are looking for a page's attachments, referencing a GUID and looking up attachments by that GUID, as you will need to look up in the media library. the UI), and scans them for any /getattachments, /getmedia, GUID values that match attachments or media, and relative paths (ex /mysite/media/images/test.png). 

# Instructions
1. Install the XperienceCommunity.MediaLibraryMigrationToolkit package on your admin application
2. Rebuild and run your site
3. Make sure the module installed by checking the event log
4. Go to Modules -> search for the migration toolkit module, edit it and add it to the current site(s)
5. Use the UI to add any Database tables + fields that may contain links to media files/attachments or GUIDs of existing media files or attachments
6. [Configure and enable Azure Blob Storage](https://docs.xperience.io/custom-development/working-with-physical-files-using-the-api/configuring-file-system-providers/configuring-azure-storage), here's a sample [configuration for the MVC](https://github.com/KenticoDevTrev/XperienceCommunity.Baseline/blob/master/starting-site/kx13/MVC/MVC/Configuration/AzureBlobStorageModule.cs), you'll need a matching on in the admin as well.
7. When ready, backup your database, and run the conversion.*

* It's best to run this locally, then push up the database changes.  If your site is 'live', i would plan a content freeze window, clone the production database, bring it locally, run these operations, then push back up the production database ALONG WITH your azure shared blob storage configuration.  You can push the azure configuration first, then restore the database to prevent 'down time'

