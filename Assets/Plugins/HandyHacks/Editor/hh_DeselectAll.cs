using UnityEngine;
using UnityEditor;

namespace HandyHacks
{
    /// <summary>
    /// Deselects all selected objects in the hierarchy and project window.
    /// </summary>
    static class hh_DeselectAll
    {
        // commented all this out as it was causing an error message
        //[MenuItem("Edit/Deselect All " + hh_Hotkeys.keyBinding_DeselectAll, false, 0)]
        //static void Deselect()
        //{
        //    if (Selection.objects.Length != 0)
        //    {
        //        foreach (Object o in Selection.objects)
        //        {
        //            Undo.RegisterFullObjectHierarchyUndo(o, "Deselect All");
        //        }
        //        Selection.objects = new Object[0];
        //    }
        //}
    }
}
