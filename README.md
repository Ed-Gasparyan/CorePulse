CorePulse - Real-Time System Monitor & Process Manager
CorePulse is a high-performance system monitoring tool built with .NET. It allows users to track active system processes, monitor CPU and RAM usage in real-time, and manage system resources through a web interface.

Key Features
Real-Time Data Streaming: Uses SignalR to push system metrics and logs to the client without page refreshes.

Process Management: Provides a detailed list of system processes including PID, Name, Memory, and Thread counts.

Administrative Control: Allows authorized users to terminate (Kill) system processes directly from the browser.

Live Activity Logs: Features a real-time logging system that tracks and displays system events, such as successful process terminations or connection statuses.

Secure Authentication: Implements JWT-based authentication with role-based access control to ensure only Admins can perform sensitive actions.

Optimized UI: Utilizes Blazor's virtualization component to handle large lists of system processes efficiently without performance degradation.

Tech Stack
Frontend: Blazor WebAssembly.

Backend: ASP.NET Core Web API & SignalR.

Database: Entity Framework Core with SQL Server.

Communication: SignalR (WebSockets) for bi-directional real-time updates.

Security: JSON Web Tokens (JWT) for secure API and Hub access.
