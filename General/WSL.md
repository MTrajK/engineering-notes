# Working with Windows WSL

WSL (Windows Subsystem for Linux) is a feature in Windows that enables you to run a Linux environment directly within Windows.
WSL offers a way to bridge the gap between Windows and Linux environments, providing developers and users with the flexibility to leverage the strengths of both operating systems on a single machine.

**Important:**
If you are using pure WSL (without WSLg), you cannot run Linux GUI applications. In that configuration, WSL is effectively CLI-only.
Intended use: CLI, dev tools, servers.

When WSL is used in its original or minimal form:
- No display server (X11 / Wayland)
- No compositor
- No GUI integration with Windows

As a result:
- Linux graphical applications cannot start
- Only command-line tools function

## WSLg - Windows Subsystem for Linux GUI
https://github.com/microsoft/wslg

WSLg enable support for running individual Linux GUI application (not all of them) on Windows in a fully integrated desktop experience (but still many of the apps can have issues). 

So WSL + WSLg is not Full Ubuntu Desktop!

| WSL Configuration | Linux GUI Apps | Intended Use |
|-------------------|----------------|--------------|
| WSL (no WSLg)     | No             | CLI, dev tools, servers |
| WSL + WSLg        | Yes (individual apps) | Dev + light GUI |
| Full Ubuntu Desktop| Yes           | Desktop computing |


## Installation
https://learn.microsoft.com/en-us/windows/wsl/install

### Step 1:
Install the WSL\
Open windows terminal/powershell
```ps
wsl --install
```
Verify WSL installed
```ps
wsl --version
```

### Step 2:
List the available distros for installation\
In windows terminal/powershell
```ps
wsl --list --online
```

### Step 3:
Install some of the distros (example Ubuntu)\
In windows terminal/powershell
```ps
wsl --install --web-download -d Ubuntu
```
'Ubuntu' vs 'Ubuntu-24.04' or '-||-22.04' or '-||-20.04' (Just 'Ubuntu' is the latest stable version, the other 3 are specific versions of Ubuntu)\
Verify Installed distros
```ps
wsl -l -v
```

### Step 4:
Launch via windows terminal/powershell using the command
```ps
wsl -d Ubuntu
```
Check the Ubuntu version
```ps
lsb_release -a
```

### Step 5:
Exit the wsl\
Just write in windows terminal/powershell
```ps
exit
```

### Step 6:
Instead using powershell, just use Windows Terminal. There is already an option to enter the Ubuntu wsl directly from the tabs

### Step 7:
To open your WSL project in Windows File Explorer enter
```ps
explorer.exe .
```
(Note: be sure to add a period at the end of the command to open the current directory.)\
Path: \\wsl.localhost\Ubuntu\home\mtrajkovski

#### Resources
- https://learn.microsoft.com/en-us/windows/wsl/setup/environment
- https://learn.microsoft.com/en-us/windows/wsl/filesystems

### Step 8:
Store your project files on the same operating system as the tools you plan to use.\
For example, when storing your WSL project files: \\wsl$\<DistroName>\home\<UserName>\Project

### Step 9:
Setup the Visual Studio Code for WSL\
Install "Remote Development" extension

#### Resources
- https://learn.microsoft.com/en-us/windows/wsl/tutorials/wsl-vscode

### Step 10:
Generally, commands follow this syntax:
```sh
command [OPTIONS] arguments
```

Most used terminal commands
- ```pwd``` (To check the path to the current work directory)
- ```cd``` (To browse to a certain path use the cd command followed by the path, ```cd ~``` takes you to the home directory)
- ```ls``` (Lists the content of the current directory)
- ```mkdir <folder-name>``` (Creates a directory)
- ```touch <file-name>``` (Creates a file)
- ```rm <file-name>``` (Removes a file)
- ```rm -r <directory-path>``` (Removes a directory with all its content)
- ```cp <file-name> <target-directory>``` (Copies file to another directory)
- ```mv <file-name> <target-directory>``` (Moves file to another directory)
- ```<command> --help``` or ```man <command>``` (To get all the descriptions of any commands)

### Step 11:
Install programms/tools in WSL Ubuntu.

Using apt
- ```sudo apt-get update``` (refreshes your system's list of available software packages and their versions from online repositories)
- ```sudo apt-get -y install <name of the program/tool>```

Search for software
- ```sudo apt search <name of the program/tool>``` (here sudo is not needed, sudo is used when permissions need to be granted)

Upgrade programm/tool
- ```sudo apt-get --only-upgrade install <name of the program/tool>```

Verify for installed
- ```sudo apt list --installed <name of the program/tool>```

See all installed
- ```sudo apt list --installed```

### Step 12:
Open some web application deployed in the WSL Ubuntu.\
In the windows env (outside the Ubuntu), just enter the url togheter with the port in the browser (example: http://localhost:3000 or http://127.0)