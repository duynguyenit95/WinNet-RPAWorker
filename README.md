# WinNet.RPAWorker

## Introduction

**WinNet.RPAWorker** is a .NET-based Robotic Process Automation (RPA) solution designed for automating repetitive tasks on Windows platforms. The project is structured across modular layers—including core logic, deployment tools, a web server, and worker services—to enable scalable and maintainable RPA workflows.

## Tech Stack

- **.NET (C# & VB.NET)** – The main implementation is in C#, with supporting modules in Visual Basic .NET, enabling robust automation workflows (69% C#, 27% VB.NET) :contentReference[oaicite:1]{index=1}.
- **Modular Project Structure**:
  - **Core (RPA.Core)** – Houses the core automation logic.
  - **Worker Manager (RPA.WorkerManager)** – Manages worker processes via Windows services.
  - **Web Server (RPA.Web)** – Provides web-based control interfaces or APIs.
  - **Deployment Tools (AutoDeployWorker)** – Streamlines deployment of worker services.
- **Solution File (.sln)** – Central entry point for development and organization.
