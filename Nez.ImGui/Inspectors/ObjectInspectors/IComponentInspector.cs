namespace Nez.ImGuiTools.ComponentInspectors
{
    public interface IComponentInspector
    {
        Entity entity { get; }
        Component component { get; }

        void draw();
    }
}
