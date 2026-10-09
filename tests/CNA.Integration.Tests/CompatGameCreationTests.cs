using System.Diagnostics;
using Xunit;

namespace CNA.Integration.Tests;

/// <summary>
/// A game whose graphics device cannot be created, in a process of its own with no display to
/// reach: XNA's GraphicsDeviceManager.CreateDevice reports that as NoSuitableGraphicsDeviceException
/// ("Unable to create the graphics device."), which a game catches to say the machine cannot run it.
/// CNA creates the device with the game, so the XNA Game constructor throws it. Before CNA
/// <c>Game.cpp</c> and the facade's guard, a <c>CnaException</c> with result Platform escaped.
/// </summary>
public class CompatGameCreationTests
{
    [NativeFact]
    public void CompatGame_WithNoDisplay_ThrowsNoSuitableGraphicsDeviceException()
    {
        string runtimeDirectory = Directory.CreateTempSubdirectory("cna-no-display-").FullName;
        try
        {
            var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            start.ArgumentList.Add("exec");
            start.ArgumentList.Add(typeof(Program).Assembly.Location);
            start.ArgumentList.Add(Program.ConstructGame);
            // No X server, no compositor: nothing a window could appear on, the owner's desktop
            // included. SDL is kept to those two drivers, so it does not reach for the console.
            start.Environment["DISPLAY"] = ":64999";
            start.Environment["WAYLAND_DISPLAY"] = "cna-no-such-compositor";
            start.Environment["XDG_RUNTIME_DIR"] = runtimeDirectory;
            start.Environment["SDL_VIDEO_DRIVER"] = "x11,wayland";
            start.Environment.Remove("DBUS_SESSION_BUS_ADDRESS");

            using Process child = Process.Start(start)!;
            Task<string> error = child.StandardError.ReadToEndAsync();
            string[] lines = child.StandardOutput.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.True(child.WaitForExit(60_000), "the child process did not finish");
            Assert.True(child.ExitCode == 0, $"exit {child.ExitCode}: {error.Result}");

            if (lines[0] == "constructed")
            {
                // A renderer that draws on the CPU creates its device without a window server, so
                // "no display" does not make it fail -- measured with SOFTWARE on macOS, where SDL
                // also has neither x11 nor wayland (CNA plans/plan_apple_m4.md AM4-224). The claim
                // this test makes is about a renderer that cannot, which every other one is held to.
                Assert.True(lines.Length > 1, "the child constructed a game but named no renderer");
                Assert.Contains(lines[1], new[] { "SOFTWARE", "HEADLESS" });
                return;
            }

            Assert.Equal(typeof(Microsoft.Xna.Framework.Graphics.NoSuitableGraphicsDeviceException).FullName, lines[0]);
            Assert.StartsWith("Unable to create the graphics device. ", lines[1]);
        }
        finally
        {
            Directory.Delete(runtimeDirectory, recursive: true);
        }
    }
}
