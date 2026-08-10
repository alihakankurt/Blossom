# Blossom

Blossom is a Discord bot that provides a variety of features for entertainment and utility. It is written in C# using [Discord.Net](https://github.com/discord-net/Discord.Net) library. It has its own [Lavalink](https://github.com/lavalink-devs/Lavalink) client, which is a library that provides playing music in Discord voice channels.

## Prerequisites

- [.NET 10.0](https://dotnet.microsoft.com/en-us/download)
- [JDK 17 LTS](https://www.oracle.com/java/technologies/downloads/) (or higher)
- [Lavalink 4.2.2](https://github.com/lavalink-devs/Lavalink/releases)
- [application.yml](https://github.com/lavalink-devs/Lavalink/blob/master/LavalinkServer/application.yml.example)

## Features

- Playing music
- Server management
- Providing information about servers and users
- Fun commands

## Installation

1. Clone the repository
2. Build the solution (using Visual Studio or dotnet CLI)
3. Create a `blossom.cfg` file in the same directory of the executable
4. Set the configuration fields (see Configuration section)
5. Configure the `application.yml` file (lavalink original documentation)
6. Run Lavalink server
7. Run the bot

## Configuration

- `Token`: token of the Discord bot application from developer dashboard
- `UserStatus`: user status; one of: Offline, Online, Idle, AFK, DoNotDisturb, Invisible
- `ActivityType`: activity type; one of: Playing, Streaming, Listening, Watching, CustomStatus, Competing
- `Activity`: activity; a text value as activity message
- `StreamUrl`: url to the stream, only set if activity type is streaming

```txt
Token: <token>
Status: Online
Activity: Hollow Knight: Silksong
ActivityType: Playing
```

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.
