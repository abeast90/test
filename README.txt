SmartYouTubeDownloader
======================

This repository contains a Windows Forms application targeting .NET 6 that drives
[yt-dlp](https://github.com/yt-dlp/yt-dlp) and [FFmpeg](https://ffmpeg.org/) to fetch
YouTube metadata and download videos or audio.  It also offers an optional
"Premiere compatibility" mode that re-encodes the download to a CFR H.264/AAC MP4
suitable for Adobe Premiere imports.

## Repository layout

- `SmartYouTubeDownloader.sln` – Solution file to open in Visual Studio 2022 or later.
- `SmartYouTubeDownloader/SmartYouTubeDownloader.csproj` – WinForms project file.
- `SmartYouTubeDownloader/*.cs` – UI logic, services, and models used by the app.

If you only saw `README.txt` after cloning previously, make sure you pulled the
latest `main` branch.  The solution and source files live alongside this README.

## Building

1. Install the .NET 6 SDK or newer.
2. Open `SmartYouTubeDownloader.sln` in Visual Studio and select **Build → Build Solution**.
   Alternatively, run `dotnet build SmartYouTubeDownloader.sln` from the command line.

## Running

1. Press **F5** in Visual Studio or execute `dotnet run --project SmartYouTubeDownloader`.
2. Provide a YouTube URL and click **Fetch** to load metadata.
3. Adjust any advanced options (quality selection, batch queue, proxy, Premiere mode).
4. Click **Download** to save the video or audio.

The application automatically locates or bootstraps `yt-dlp.exe` and `ffmpeg.exe` and
presents unified progress for download and (optional) transcoding stages.

## Publishing your local clone back to GitHub

If you cloned an empty repository on GitHub (for example, it only contained
`README.txt`) and now want to upload the SmartYouTubeDownloader solution that
you have locally:

1. **Verify the files are present locally.** In Visual Studio’s Solution
   Explorer, you should see `SmartYouTubeDownloader.sln` and the
   `SmartYouTubeDownloader` project folder with its `.cs` files.
2. **Check Git status.** In Visual Studio, open **Git Changes** (View → Git
   Changes). The new files should appear under *Changes*. Alternatively, run
   `git status` in a terminal at the repository root.
3. **Stage and commit.** Select all of the new files, enter a commit message
   (e.g., “Add SmartYouTubeDownloader WinForms app”), and press **Commit** in
   Visual Studio, or run:

   ```bash
   git add .
   git commit -m "Add SmartYouTubeDownloader WinForms app"
   ```

4. **Push to GitHub.** Click **Push** in Visual Studio’s Git Changes window, or
   run `git push origin main` from the terminal. Visual Studio will prompt for
   GitHub sign-in if needed.
5. **Refresh the GitHub page.** After the push completes, reload your GitHub
   repository in the browser—you should now see the solution, project files,
   and source code alongside `README.txt`.

If GitHub still only shows `README.txt`, ensure you committed to the correct
branch (usually `main`) and pushed to the same remote URL that GitHub lists for
the repository.

> **Need a printable checklist?** See `PUSH_TO_GITHUB.md` for a copy/paste set of
> instructions you can follow step-by-step when preparing your repository.
