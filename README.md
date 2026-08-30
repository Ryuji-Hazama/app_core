# Application Core

I will show you about a class library `App.Core` for `C#` application core in this README file.

## :memo: Overview

The `App.Core` is a `C#` class library designed to provide core functionalities for applications. It includes components, utilities, and services that can be reused across different projects, promoting code reusability and maintainability.

### :sparkles: Core Features

- **Components Library**: A collection of reusable components that can be easily integrated into applications, and injects dependencies into the application without build entire application.
- **Logger**: A logging utility that allows developers to log messages, errors, and other information for debugging and monitoring purposes. (Note: The logger is not implemented yet.)

## :arrow_down: Clone the Repository

We are temporarily using a git bundle to manage the repository. But we will switch to a normal git repository in the future. Please clone the repository using the following command:

```bash
git clone /path/to/app_core.bundle app_core
```

## :package: Importing the Library to Your Project

To use the `App.Core` library in your `C#` project, follow these steps:

### :arrow_down: Import Library to Aras Innovator Server Method

1. Copy the `App.Core.dll` file to the `bin` directory of your Aras Innovator server.
2. In the Aras Innovator client, go to **Administration** > **Server Methods**.
3. Create a new server method or edit an existing one.
4. In the **References** section, add a reference to the `App.Core.dll` file.
5. Save the server method. You can now use the classes and methods from the `App.Core` library in your server method code.

### :arrow_down: Import Library to Your Local `C#` Project

1. Copy the `App.Core.dll` file to a suitable location in your local project directory.
2. In your local `C#` project, add a reference to the `App.Core.dll` file.
   - If you are using Visual Studio:
     - Right-click on your project in the Solution Explorer.
     - Select "Add" > "Reference..."
     - Browse to the location of the `App.Core.dll` file and add it.
     - Click "OK" to close the **Reference Manager** dialog. You can now use the classes and methods from the `App.Core` library in your local project code.
   - If you are using the .NET CLI, you can add a reference to the `App.Core.dll` file by running the following command in your project directory:

   ```bash
   dotnet add reference /path/to/App.Core.dll
   ```

   - Or, if you are using a project file (`.csproj`), you can manually add the reference by editing the project file and adding the following line within the `<ItemGroup>` section:

   ```xml
   <Reference Include="App.Core">
       <HintPath>/path/to/App.Core.dll</HintPath>
   </Reference>
   ```

3. Save the changes to your project file. You can now use the classes and methods from the `App.Core` library in your local project code.

## :gear: Usage

### Components Library

The `Components Library` provides dinamic dependency injection for your application. You can register and resolve components using the provided methods. For example:

1. Receive or create a class interface in the `App.Contracts` namespace.
2. Create a injection mapping configuration.

    ```json
    {
        "Components": [
            {
                "Name": "ClassA",
                "Type": "App.ClassA",
                "Assembly": "App.ClassA.dll",
                "Context": "App.ClassA",
                "Interface": "App.Contracts.IClassA",
            }
        ]
    }
    ```

3. Edit or create `App.json` file in your application root directory and set path to the injection mapping configuration file.

    ```json
    {
        "App": {
            "AssemblyPath": "/path/to/assembly/directory",
            "ComponentsList": "/path/to/injection/mapping/configuration/file.json"
        }
    }
    ```

4. Use the `ComponentsLibrary` class to register and resolve components in your application. For example:

```csharp
using App.Contracts;
using App.Core;

IComponentsLibrary componentsLibrary = new ComponentsLibrary();
IClassA classA = componentsLibrary.GetComponent<IClassA>("ClassA");
classA.SomeMethod();
```

### Logger

The `Logger` utility allows developers to log messages, errors, and other information for debugging and monitoring purposes. (Note: The logger is not implemented yet.)
