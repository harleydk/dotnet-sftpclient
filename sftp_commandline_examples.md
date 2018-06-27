### Powershell examples of use of the commandline-tool:

#### The following examples may be of inspiration in testing-scenarios.



Upload single text test file, by way of a private key file that matches the server's public key:
```powershell
./dotnetsftp --tt=upload --host=<your-host> --port=2 --username=tester --password=testPW --pk=<private_key_filePath> --dp=<data_path_on_the_sftp_server> --sp=c:\temp\test.txt --ow
```

Upload single text test file, different server:
```powershell
./dotnetsftp --tt=upload --host=<your-host> --port=22 --username=tester --password=testPW --pk=<private_key_filePath> --dp=/temp --sp=c:\temp\test.txt --ow
```

Upload single text test file, save settings file:
```powershell
./dotnetsftp --tt=upload --host=<your-host> --port=22 --username=tester --password=testPW --pk=<private_key_filePath> --dp=/temp --sp=c:\temp\test.txt --ow --sf=C:\Temp\uploadTest.settings
```

Upload single text test file, import settings from only settings file (persisted above):
```powershell
./dotnetsftp --sf=C:\Temp\uploadTest.settings
```

Upload single text test file, save settings file, encrypt it, save encryption file:
```powershell
./dotnetsftp --tt=upload --host=<your-host> --port=22 --username=tester --password=testPW --pk=<private_key_filePath> --dp=<data_path_on_the_sftp_server> --ow --sf=C:\Temp\uploadTest.settings --sfKey=C:\Temp\uploadTest.settingsKey
```

Upload single text test file, import settings from settings file, decrypt using key:
```powershell
./dotnetsftp --sf=C:\Temp\uploadTest.settings --sfKey=C:\Temp\uploadTest.settingsKey
```

Upload single text test file - using network-paths:
```powershell
./dotnetsftp --tt=upload --host=<your-host> --port=22 --username=tester --password=testPW --pk=<private_key_filePath> --dp=<data_path_on_the_sftp_server> --ow
```

Upload single text test file, save settings file, encrypt it, save encryption file - using network-paths:
```powershell
./dotnetsftp --tt=upload --host=<your-host> --port=22 --username=tester --password=testPW --pk=<private_key_filePath> --dp=<data_path_on_the_sftp_server> --ow --sf=<source_file_path> --sfKey=<path_to_the_settings_key>.settingsKey
```

Upload single text test file, import settings from only settings file (persisted above):
```powershell
./dotnetsftp  --sf=<path_to_settings_file>.settings --sfKey=<path_to_settings_key_file>.settingsKey
```

Upload entire directory, compressed and with checksum:
```powershell
./dotnetsftp --tt=upload --host=<your-host> --port=22 --username=ftpt_blueprismu --password=testPW --pk=<private_key_filePath> --dp=<data_path_on_the_sftp_server> --sp=c:\temp\test.txt --ow --cs --cd
```

