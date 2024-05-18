# Blossom

Blossom is a Discord bot that provides a variety of features for entertainment and utility. It is written in C# using [Discord.Net](https://github.com/discord-net/Discord.Net) library. It has its own wrapper for [Lavalink](https://github.com/lavalink-devs/Lavalink), which is a music player for Discord.

## Prerequisites

- [.NET 8.0](https://dotnet.microsoft.com/en-us/download)
- [JDK 17](https://www.oracle.com/java/technologies/downloads/) (or higher)
- [Lavalink 4.0.5](https://github.com/lavalink-devs/Lavalink/releases)
- [application.yml](https://github.com/lavalink-devs/Lavalink/blob/master/LavalinkServer/application.yml.example)

## Features

- Playing music
- Moderation commands
- Fun commands
- Information commands

## Installation

1. Clone the repository
2. Build the solution (using Visual Studio or dotnet CLI)
3. Create a `config.cfg` file in the root directory of the bot
4. Set the configuration fields (see Configuration section)
5. Run Lavalink server
6. Run the bot

## Configuration

1. Create a `config.cfg` file in the root directory of the bot
2. Set the `Token` field to your bot token
3. Set the `Version` field to the version of the bot
4. Set the `Status` field to the status of the bot
5. Set the `Activity` field to the activity of the bot
6. Set the `ActivityType` field to the activity type of the bot

Example `config.cfg` file:
```config
Token=<token>
Version=1.0.0
Status=Online
Activity=with flowers
ActivityType=Playing
```

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.
