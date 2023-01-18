This is a file with information to the structure of the project

- Assets: contains all new files
    - Scenes: contains all game scenes
    - Scripts: contains all c# scripts
        - Game: contains all scripts for the game scene
            - GameLogic.cs: contains the logic behind the sing screen
            - GameState.cs: contains the game state f.e. choosen song and player names
            - MicrophoneInput.cs: converts the microphone input to a node
            - VideoPlayer.cs: plays the music video
        - MainMenu: contains scripts for the main menu
            - MainMenu.cs: script for managing the main menu
        - SongEnd: scripts for the song end scene
            - SongEnd.cs: script for managing the song end scene
        - Classes.cs: contains all classes and enums used in multiple cs files
    - UI: contains all ui relevant items
        - GameOverlays: contains the uxml and uss files
        - UITextures: contains all textures used for UI