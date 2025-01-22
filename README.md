If the Application Arguments option is not available in Visual Studio, you can pass command-line arguments manually via the terminal. Here's how to run your program:
1.  Build the Project: In Visual Studio, press Ctrl+Shift+B to build your project. Ensure the build succeeds without errors.
2.	Locate the Executable File: Navigate to the bin/Debug/net8.0 folder inside your project directory. This is where the compiled executable (NonTransitiveDiceGame.exe) is located.
3.	Run the Program via Terminal: Open a terminal or command prompt. Change to the directory where the executable is located: cd path\to\bin\Debug\net8.0 Run the program with the required arguments: NonTransitiveDiceGame.exe 2,2,4,4,9,9 6,8,1,1,8,6 7,5,3,7,5,3
4.	Test the Input: Make sure you provide at least three sets of dice, each as a comma-separated list of integers. If the input is valid, the program will start and guide you through the gameplay.

Notes: If you want to pass arguments directly in Visual Studio (and this feature is missing in your version), you may need to upgrade Visual Studio or use a .runsettings file.
