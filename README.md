# Ghost Admin API Integration

This repository hosts a small console application that demonstrates how to automate common tasks using the [Ghost Admin API](https://ghost.org/docs/admin-api/). The commands provided here let you bulk import pages and members into a Ghost site.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/) installed and available on your `PATH`.
- A Ghost instance with a valid **staff access token**.

## Building

Run the following command in the repository root to compile the project:

```bash
 dotnet build
```

## Usage

The console project lives in the `src` folder. Use `dotnet run` with `--project src` to execute commands.

### Import Pages

```bash
dotnet run --project src -- import-pages --url <ghost-url> --key <staff-token> --input <pages.json>
```

### Import Members

```bash
dotnet run --project src -- import-members --url <ghost-url> --key <staff-token> --input <members.json>
```

Both commands expect a JSON file with an array of objects.

### Page JSON Structure

`pages.json` should contain objects matching the following structure:

```json
[
  {
    "Path": "path/to/file",
    "Name": "filename",
    "Day": "dd",
    "Month": "mm",
    "Date": "2024-01-01T00:00:00Z",
    "Title": "Page Title",
    "Summary": "Brief summary",
    "Content": "HTML content"
  }
]
```

### Member JSON Structure

`members.json` must contain objects of this form:

```json
[
  {
    "Email": "member@example.com",
    "Name": "Member Name",
    "CreatedAt": "2024-01-01T00:00:00Z"
  }
]
```

Refer to the [Ghost Admin API documentation](https://ghost.org/docs/admin-api/) for details about authentication and available endpoints.
