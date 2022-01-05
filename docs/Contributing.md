# Contributing

Contributions are welcome! You can contribuite to gpm with issues, ideas and PRs. Simply filling issues or propose new ideas is a great way to contribute.

You can begin by looking through the issues - issues tagged with `good first issue` are a great way to start! 

## git workflow

We currently use a simplified git workflow for fast development: Development happens from feature and fix branches off the dev branch. The dev branch may be periodically merged into main. 

Releases for gpm are created automatically when a tag is pushed to main. Releases also automatically push the dotent tool to nuget.

The main branch is protected: 
- Pull requests are needed to commit to main, 
- and a positive review from a code owner.

The dev branch is protected: 
- Pull requests are needed to commit to main, 
- and a positive review from a contributor.



## Backend

gpm's backend (gpm.core.csproj) is written in **c# NET6**. 

We use a VS editor config file, please adhere to the code style defined in there.

## Frontend

gpm's frontend is currenty Windows-only. 

We use **WINUI 3** for the UI. 
> more info: https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=vs-2022

We use the **MVVM** pattern for the UI. (specifically the Microsoft mvvm toolkit)
> more info: https://docs.microsoft.com/en-us/windows/communitytoolkit/mvvm/introduction


