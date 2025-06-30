public class SceneContext<T> where T : SceneContext<T>, new()
{
    static T _instance;
    public static T Instance
    {
        get
        {
            _instance ??= new T();
            return _instance;
        }
    }
}
