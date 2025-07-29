using Sirenix.OdinInspector;

[LabelText("Task: Unlock Class")]
public class Task_UnlockClass<T> : ITaskOwned<Viewer, T>
{
    public string classUnlock;
    public bool DoTask(Viewer viewer, dynamic data)
    {
        viewer.unlockedClasses.Add(classUnlock);
        return true;
    }
}