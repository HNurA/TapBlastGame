# 🎮 Block Collapse Game
A fun and engaging block-matching game built with Unity! Tap to remove matching blocks, watch them collapse with gravity, and try to maximize your score.

## ✨ Features
- **Tap to Remove**: Click on groups of 2 or more matching blocks to remove them.
- **Gravity-Based Collapse**: Blocks fall dynamically to fill empty spaces.
- **Adaptive Appearance**: Blocks change appearance based on group size.
- **Deadlock Detection**: Efficient algorithm to reshuffle when no moves are available.
- **Score System**: Earn points based on the size of the block group destroyed.
- **Restart Game**: Reset the board and start fresh anytime.

## 🛠️ Built With
- **Unity** (2D) – Game development framework
- **C#** – Core scripting language
- **TextMeshPro** – UI text rendering

## 🚀 Getting Started
1. Clone the repository:
   ```sh
   git clone https://github.com/yourusername/block-collapse-game.git
   ```
2. Open the project in Unity.
3. Press **Play** to start testing!

## 📂 Project Structure
- `Block.cs` - Handles block properties and interactions.
- `BoardManager.cs` - Manages grid logic, falling blocks, and deadlock detection.
- `GameManager.cs` - Handles score system and game resets.

## 🎯 How to Play
1. Click on any group of 2 or more adjacent blocks of the same color.
2. The blocks will disappear, and remaining blocks will fall.
3. Earn points based on the number of blocks cleared.
4. The game will detect deadlocks and reshuffle if needed.
5. Try to get the highest score possible!

## 🌟 Contributions
Contributions, issues, and feature requests are welcome! Feel free to fork the project and submit pull requests.

Happy coding! 🎮
