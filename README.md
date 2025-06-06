# MnM Journal Reader

A desktop application for reading and searching through conversation logs from the game "Monsters and Memories".

![Main application interface](screenshots/main-interface.png)

## Overview

MnM Journal Reader provides an easy way to browse and search through your in-game conversations with NPCs. The application automatically locates your game's journal files and allows you to explore conversations by character and NPC.

## Download

You can download the fully compiled version of MnM Journal Reader here:
[Download MnM Journal Reader](https://drive.google.com/file/d/10IaIHM4sQkjCbHKQlK9kMtNuj9XcMFlR/view?usp=drive_link)

## Features

- **Character Selection**: Choose from all your game characters
  ![Character selection dropdown](screenshots/character-selection.png)
  
- **NPC Filtering**: View conversations with specific NPCs
  ![NPC filtering](screenshots/npc-filtering.png)
  
- **Searchable Logs**: Search for specific text within the current NPC's logs
- **Global Search**: Search for text across all NPCs for the selected character
  ![Search functionality](screenshots/search-feature.png)
  
- **Sortable Entries**: Click column headers to sort by date, NPC name, or message content
  ![Sortable columns](screenshots/sorting-columns.png)
  
- **Clean Interface**: Simple, easy-to-use design that displays conversations clearly

## How to Use

1. **Launch the Application**: Open MnM Journal Reader
2. **Select a Character**: Choose one of your game characters from the dropdown
3. **Choose an NPC**: Select an NPC to view your conversations with them
4. **Search**: Use the search box to find specific conversations
   - Click "Search Current NPC" to search only within the current NPC's logs
   - Click "Search All NPCs" to find matching conversations across all NPCs

## Technical Details

- The application automatically looks for journal files in:
  ```
  %LocalAppData%\..\LocalLow\Niche Worlds Cult\Monsters and Memories\journal\
  ```
- Log entries are parsed and displayed with timestamps, NPC names, and conversation text
- Entries can be sorted by clicking column headers

## Requirements

- Windows operating system
- .NET Runtime
- "Monsters and Memories" game with journal entries

## Troubleshooting

If no characters or NPCs appear:
- Ensure you have played "Monsters and Memories" and have conversation logs
- Check if the journal directory exists at the expected location
- The application requires at least one character folder with NPC log files
