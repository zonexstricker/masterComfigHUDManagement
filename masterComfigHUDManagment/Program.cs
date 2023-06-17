using LibGit2Sharp;
using System;
using System.Diagnostics;
using System.Text.Json;

namespace masterComfigHUDManagment
{
    public class Program //this is used when a HUD is to be cloned
    {
        public string dbFilePath;
        public string userTf2FolderPath;
        public string tf2args;

        public void NormalStart()
        {
            Main();
        }
        private static void Main()
        {
            

            Program instance = new Program();
            string documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            bool fileExists = File.Exists(documentsFolderPath + "\\HUDManagment\\config.config");
            
            if (fileExists) //since the file exists it finds the two file paths (tf2 folder and database folder) from it and stores as variables
            {
                string[] filePaths = File.ReadAllLines(documentsFolderPath + "\\HUDManagment\\config.config");

                instance.userTf2FolderPath = filePaths[0];
                instance.dbFilePath = filePaths[1];

                

            }
            else
            {
                instance.firstTimeSetUp();
            }
            instance.mainMenu();
        }

        public void firstTimeSetUp()
        {
            Console.WriteLine("What is your tf2 directory (ends in ...|steamapps|common|Team Fortress 2");
            userTf2FolderPath = Console.ReadLine(); //TODO: Validation & verification

            Console.WriteLine("What is your database directory (must contain hud-data folder)");
            dbFilePath = Console.ReadLine();

            Console.WriteLine("If you have any, what tf2 args do you use (type in this format-novid -noborder or press enter)");
            tf2args = Console.ReadLine();
            if (tf2args.Length == 0) //empty variable means the program goes out of bounds when looking for an argument, this way it doesnt break
            {
                tf2args = " ";
            }

            //find users documents folder, then creates the configuration file
            string configFolderFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\HUDManagment";
            Directory.CreateDirectory(configFolderFilePath);
            string configFilePath = configFolderFilePath + "\\config.config";
            using (StreamWriter writer = new StreamWriter(configFilePath))
            {
                writer.WriteLine(userTf2FolderPath);
                writer.WriteLine(dbFilePath);
                writer.WriteLine(tf2args);
            }



        }
        public string getCurrentHUDName()
        {
            string filepath = userTf2FolderPath + "\\tf\\custom\\hud.path";
            string currentHUD = File.ReadAllText(filepath);
            return currentHUD;
        }
        public void mainMenu()
        {
            Console.WriteLine("#1| Install a new HUD");
            Console.WriteLine("#2| Check for / update current HUD");
            Console.WriteLine("#3| Uninstall current HUD");
            Console.WriteLine("#4| Reset config");

            string userInput = Console.ReadLine();

            bool hudInstalled = File.Exists(userTf2FolderPath + "\\tf\\custom\\hud.path");

            switch (userInput)
            {
                case "1":
                    InstallHUD(selectHUD());
                    mainMenu();
                    break;
                case "2":
                    if (hudInstalled)
                    {
                        Update();
                    }
                    else
                    {
                        Console.WriteLine("Please install a HUD first.");
                    }
                    mainMenu();
                    break;
                case "3":
                    
                    if (hudInstalled)
                    {
                        Console.WriteLine("Are you sure you want to uninstall the current HUD? (This action is irreversible) [Y] Yes, [N] No.");
                        string uninstallYN = Console.ReadLine();
                        if (uninstallYN.ToUpper() == "Y")
                        {
                            Delete(userTf2FolderPath + "\\tf\\custom\\" + getCurrentHUDName());
                        }
                        else mainMenu();
                    }
                    else
                    {
                        Console.WriteLine("No HUD installed."); //this can also happen if the hud.path file is missing but lets tackle that later.
                         mainMenu();
                    }
                    mainMenu();
                    break;
                case "4":
                    string configFolderFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\HUDManagment";
                    Directory.CreateDirectory(configFolderFilePath);
                    string configFilePath = configFolderFilePath + "\\config.config";
                    File.Delete(configFilePath);
                    Console.WriteLine("Config file deleted, going to first time set-up");
                    firstTimeSetUp();
                    mainMenu();
                    break;
                    
            }
        }

        public string selectHUD()
        {
            string directoryPath = dbFilePath + "\\hud-data";
            string[] files = Directory.GetFiles(directoryPath);

            Console.WriteLine("Files:");
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                Console.WriteLine(fileName);
            }
            Console.WriteLine("Type in the name of the HUD you wish to install");
            string hudName;
            hudName = Console.ReadLine();
            return hudName;
        }

        public void InstallHUD(string hudName)
        {
            Console.WriteLine("This will delete any existing HUD, continue? [Y] Yes, [N] No.");
            string userInput = Console.ReadLine();
            if (userInput.ToUpper() == "Y")
            {

                bool fileExists = File.Exists(userTf2FolderPath + "\\tf\\custom\\hud.path");

                if (fileExists)
                {

                    Delete(userTf2FolderPath + "\\tf\\custom\\" + getCurrentHUDName()); //deletes the currently installed HUD 
                    Console.WriteLine("Old HUD has been deleted");
                }

                Clone(hudName, userTf2FolderPath + "\\tf\\custom\\" + hudName);

                Console.WriteLine(hudName + " has been installed successfully!");


            }
            else if (userInput.ToUpper() == "N")
            {
                Console.WriteLine("Cancelling operation");
            }
            else
            {
                InstallHUD(hudName);
            }

        }

        public void Clone(string hudName, string HUDFilePath)
        {
            string filePath = dbFilePath + "\\hud-data\\" + hudName + ".json";
            //Clone(gitpath(based from the hudName, will need to lookup on the database), "tf2customfolder")
            string jsonContent = File.ReadAllText(filePath);
            JsonDocument jsonDocument = JsonDocument.Parse(jsonContent);

            //Extract the "repo" entry
            JsonElement repoElement = jsonDocument.RootElement.GetProperty("repo");
            string repo = repoElement.GetString();

            try
            {
                Repository.Clone(repo, HUDFilePath);

            }
            catch (Exception e)
            {
                if (String.IsNullOrEmpty(Convert.ToString(e)))
                {
                    //idk if the exception can be empty but this is just incase ig
                    Console.WriteLine("Failed to clone: No explanation available");
                }
                else
                {
                    Console.WriteLine($"Failed to clone: {e.Message}");
                }
            }

            File.WriteAllText(GetParentDirectory(HUDFilePath) + "\\hud.path", hudName); //TODO: change from gitPath to name of the hud

            //Console.WriteLine("HUD cloned successfully");
        }
        public static string GetParentDirectory(string directoryPath)
        {
            DirectoryInfo directoryInfo = Directory.GetParent(directoryPath);
            string parentDirectoryPath = directoryInfo.FullName;
            return parentDirectoryPath;
        }
        public void Update()
        {
            string filePath = dbFilePath + "\\hud-data\\" + getCurrentHUDName() + ".json";
            //Clone(gitpath(based from the hudName, will need to lookup on the database), "tf2customfolder")
            string jsonContent = File.ReadAllText(filePath);
            JsonDocument jsonDocument = JsonDocument.Parse(jsonContent);

            //Extract the "repo" entry
            JsonElement repoElement = jsonDocument.RootElement.GetProperty("repo");
            string gitPath = repoElement.GetString();


            string HUDFilePath = userTf2FolderPath + "\\tf\\custom\\" + getCurrentHUDName();

            bool isUpToDate = IsRepoUpToDate(HUDFilePath);

            if (isUpToDate)
            {
                Console.WriteLine("The repository is up to date.");
            }
            else
            {
                Console.WriteLine("The repository is not up to date. Pulling the latest changes...");

                bool isPullSuccessful = PullLatestChanges(HUDFilePath);

                if (isPullSuccessful)
                {
                    Console.WriteLine("Pull operation completed successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to pull the latest changes. Please check the repository and try again.");
                }
            }
        }

        public void Delete(string HUDFilePath)
        {
            
            try
            {

                DeleteDirectory(HUDFilePath);
                Console.WriteLine("Directory deleted successfully");
                File.Delete(userTf2FolderPath + "\\tf\\custom\\hud.path");

            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to delete directory: {e.Message}");
            }
        }

        private static void DeleteDirectory(string directory)
        {
            foreach (string subdirectory in Directory.EnumerateDirectories(directory))
            {
                DeleteDirectory(subdirectory);
            }

            foreach (string fileName in Directory.EnumerateFiles(directory))
            {
                var fileInfo = new FileInfo(fileName)
                {
                    Attributes = FileAttributes.Normal
                };
                fileInfo.Delete();
            }

            Directory.Delete(directory);
        }

        public static bool IsRepoUpToDate(string repoPath)
        {
            //Directory is the cloned directory
            Environment.CurrentDirectory = repoPath;

            //Run the 'git fetch' command to fetch the latest changes from the remote repository
            RunGitCommand("fetch");

            //Get the local and remote branch names
            string localBranch = RunGitCommand("rev-parse --abbrev-ref HEAD").Trim();
            string remoteBranch = $"origin/{localBranch}";

            //Compare the local and remote branch hashes
            string localHash = RunGitCommand("rev-parse HEAD").Trim();
            string remoteHash = RunGitCommand($"rev-parse {remoteBranch}").Trim();

            return localHash == remoteHash;
        }

        public static bool PullLatestChanges(string repoPath)
        {
            //Directory is the cloned directory
            Environment.CurrentDirectory = repoPath;

            //Run the 'git pull' command to pull the latest changes from the remote repository
            int exitCode = RunGitCommandWithExitCode("pull");

            return exitCode == 0; //0 indicates a successful execution of the git pull command
        }

        public static string RunGitCommand(string arguments)
        {
            string gitPath = "git"; //Assuming 'git' is in the system's PATH environment variable

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = gitPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output;
            }
        }

        public static int RunGitCommandWithExitCode(string arguments)
        {
            string gitPath = "git"; //Assuming 'git' is in the system's PATH environment variable

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = gitPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                return process.ExitCode;
            }
        }
    }

}

