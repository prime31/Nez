namespace Nez.ImGuiTools.ObjectInspectors
{
    public interface IComponentInspector
    {
        Entity entity { get; }
        Component component { get; }

        void draw();
    }
}
