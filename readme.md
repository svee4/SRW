# SRW
Script to randomize your Windows wallpaper

# Building and running
- For development, just run it. If you don't provide arguments, the program will ask for them
- For release, I recommend using the provided publish profile by running "dotnet publish -p:PublishProfile=FD --self-contained false"
- For daily use, I recommend making a shortcut with your favourite arguments

# Arguments
- `--help` to print help
- `--directory=<string>` or `--directory="<string>"` to specify the directory where your wallpaper files reside
- `--sleep=<int>` to specify the time in milliseconds that the program should sleep for, before checking if the wallpaper was updated succesfully
