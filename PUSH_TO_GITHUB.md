# Publishing the SmartYouTubeDownloader code to your own GitHub repository

If your GitHub repository currently only contains a placeholder `README.txt`, you
can add the SmartYouTubeDownloader application that lives in this working copy by
following the steps below. These instructions assume you already cloned your
GitHub repo locally using Visual Studio or the Git command line.

## 1. Copy the solution into your clone (if needed)

If the WinForms solution is not yet in your local clone, copy the following items
from this working directory into your clone folder:

- `SmartYouTubeDownloader.sln`
- The entire `SmartYouTubeDownloader` directory and its contents
- `README.txt` (or merge it with your existing README if desired)

Visual Studio users can do this by dragging the files in File Explorer; Git will
pick them up as new additions.

## 2. Verify the files appear in Visual Studio

1. Open the cloned repo in Visual Studio.
2. In **Solution Explorer**, you should see `SmartYouTubeDownloader.sln`.
3. Expand the `SmartYouTubeDownloader` project to confirm the `.cs` source files
   (e.g., `MainForm.cs`, `Services/`, `Models/`, etc.) are present.

If Solution Explorer is empty, close Visual Studio and re-open the solution file
from the cloned folder.

## 3. Stage and commit the files

Using Visual Studio:

1. Open **View → Git Changes**.
2. In the *Changes* list, select all newly added files.
3. Enter a commit message such as `Add SmartYouTubeDownloader WinForms app`.
4. Click **Commit All** (or **Commit All and Push** to combine steps 3 and 4).

Using the command line:

```bash
cd <path-to-your-clone>
git status           # confirm the new files appear under "Changes"
git add .
git commit -m "Add SmartYouTubeDownloader WinForms app"
```

## 4. Push to GitHub

If you used **Commit All and Push**, Visual Studio already pushed your commit.
Otherwise:

- Visual Studio: In the **Git Changes** window, click **Push**.
- Command line: run `git push origin main` (replace `main` with your default
  branch name if different).

After the push completes, refresh your GitHub repository page—you should now see
the solution and project files.

## 5. Troubleshooting tips

- **Wrong branch?** Confirm `git status` shows `On branch main` (or the branch you
  expect). If not, either switch to the correct branch (`git checkout main`) or
  push the branch you actually committed on (`git push origin <branch>`).
- **Detached HEAD?** If Visual Studio says you are in a detached state, create a
  branch first: `git checkout -b main` (if `main` does not yet exist) and then
  push.
- **Remote rejected push?** Make sure the remote URL points to your GitHub repo:
  `git remote -v`. If it does not, set it with `git remote set-url origin <url>`.
- **Need GitHub credentials?** Visual Studio will prompt you to sign in. On the
  command line, you may need to use a personal access token in place of your
  password if you have two-factor authentication enabled.

Once the push succeeds, anyone who clones your GitHub repository will receive the
full SmartYouTubeDownloader project.
