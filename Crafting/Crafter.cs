using System;
using System.Threading.Tasks;
using Dalamud.Logging;
using Peon.Managers;
using Peon.Modules;

namespace Peon.Crafting;

public class Crafter : IDisposable
{
    private readonly InterfaceManager _interface;
    private readonly CommandManager   _commands;

    public bool Verbose { get; set; }

    private int  _numCrafts;
    private int  _currentStep;
    private int  _successfulCrafts;
    private bool _running;

    public Crafter(CommandManager commandManager, InterfaceManager interfaceManager,
        bool verbose)
    {
        _commands  = commandManager;
        _interface = interfaceManager;
        Verbose    = verbose;
    }

    private void Log(string s)
    {
        PluginLog.Verbose(s);
        if (Verbose)
            Dalamud.Chat.Print(s);
    }

    private bool Error(string error, bool cancel = false)
    {
        PluginLog.Error(error);
        Dalamud.Chat.PrintError(error);
        if (cancel)
            Cancel();
        return false;
    }

    private ActionInfo Step(Macro macro)
    {
        _currentStep = _interface.Synthesis().Step;
        if (_currentStep >= macro.Count)
        {
            Error($"Reached step {_currentStep} but macro only has {macro.Count}.");
            return ActionId.None.Use();
        }

        return macro.Step(_currentStep);
    }

    public void Cancel()
    {
        _running          = false;
        _currentStep      = 0;
        _numCrafts        = 0;
        _successfulCrafts = 0;
    }

    public void ExecuteStep(Macro macro)
    {
        var action = Step(macro);
        if (_running)
        {
            _commands.Execute(action.Cast());
            Log($"{macro.Name} Step {_currentStep}: {action.Name}");
        }
    }

    public Task<bool> CraftAmount(Macro macro, int amount)
    {
        if (_running)
        {
            Error("Already crafting");
            return Task.FromResult(false);
        }

        _running   = true;
        _numCrafts = amount;
        return Task.Run(async () =>
        {
            for (var i = 0; i < _numCrafts; ++i)
            {
                if (!_running)
                {
                    Log($"Stopped after {i + 1}/{_numCrafts} crafts due to user input.");
                    Cancel();
                    return true;
                }

                if (!_interface.Synthesis() && !_interface.RecipeNote())
                    return Error($"No recipe available and not crafting at craft {i + 1}/{_numCrafts}.", true);

                var synthesis = _interface.Synthesis();
                if (!synthesis)
                {
                    var task = _interface.Add("Synthesis", true, 5000);
                    _interface.RecipeNote().Synthesize();
                    task.Wait();
                    if (!task.IsCompleted || task.Result == IntPtr.Zero)
                        return Error($"Restarting craft timed out at craft {i + 1}/{_numCrafts}: Synthesis did not reopen.", true);

                    synthesis = task.Result;
                }

                _currentStep = synthesis.Step;
                var highestStep = 1;
                if (_currentStep == 0)
                    return Error($"Not crafting anything at craft {i + 1}/{_numCrafts}.", true);

                var tries = 0;
                while (_running && _currentStep < macro.Count)
                {
                    _currentStep = _interface.Synthesis().Step;
                    if (_currentStep < highestStep)
                    {
                        Log($"Terminated craft {i + 1}/{_numCrafts} early at step {_currentStep}/{macro.Count} because steps reset.");
                        break;
                    }

                    if (highestStep == _currentStep)
                        ++tries;
                    else
                        tries = 0;

                    if (tries > 3)
                    {
                        Error(
                            $"Terminated craft {i + 1}/{_numCrafts} because action {_currentStep}/{macro.Count} could not be used after delays.");
                        break;
                    }

                    highestStep = _currentStep;
                    var action = macro.Step(_currentStep);
                    _commands.Execute(action.Cast());
                    Log($"{macro.Name} Craft {i + 1}/{_numCrafts}, Step {_currentStep}/{macro.Count}: {action.Name}");
                    await Task.Delay(action.Delay);
                }

                Log($"Craft {i + 1}/{_numCrafts} successfully completed.");
                ++_successfulCrafts;
            }

            Log($"{_successfulCrafts}/{_numCrafts} successfully completed.");
            Cancel();
            return true;
        });
    }

    public void Dispose()
    {
        Cancel();
    }
}
