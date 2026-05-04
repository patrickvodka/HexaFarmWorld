using System.Windows.Input;
using UnityEngine;

public class CommandInvoker : MonoBehaviour
{
    public static CommandInvoker Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Execute(ICommand command)
    {
        command.Execute();
    }
}