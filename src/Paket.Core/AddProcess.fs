﻿/// Contains methods for addition of new packages
module Paket.AddProcess

open Paket
open System.IO
open Paket.Domain

let Add(dependenciesFileName, package, version, force, hard, interactive, installAfter) =
    let existingDependenciesFile = DependenciesFile.ReadFromFile(dependenciesFileName)
    let dependenciesFile =
        existingDependenciesFile
          .Add(package,version)

    dependenciesFile.Save()

    let lockFile = UpdateProcess.SelectiveUpdate(dependenciesFile,Some(NormalizedPackageName package),force)
    
    if interactive then
        for project in ProjectFile.FindAllProjects(Path.GetDirectoryName lockFile.FileName) do
            let notInstalled = project.HasPackageInstalled(NormalizedPackageName package) |> not
            if notInstalled && Utils.askYesNo(sprintf "  Install to %s?" project.Name) then
                ProjectFile.FindOrCreateReferencesFile(FileInfo(project.FileName))
                    .AddNuGetReference(package)
                    .Save()

    if installAfter then
        let sources = dependenciesFile.GetAllPackageSources()
        InstallProcess.Install(sources, force, hard, false, lockFile)