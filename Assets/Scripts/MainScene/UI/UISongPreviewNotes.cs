using System.Collections.Generic;
using UnityEngine;

public class UISongPreviewNotes : MonoBehaviour
{
    public GameObject donPrefab;
    public GameObject kaPrefab;

    public void CreateNotes(PatternLanguage language)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        var (notes, _) = language.GetNotes(10, 50, 0);
        notes.Reverse();

        foreach (var note in notes)
        {
            GameObject prefab = note.type == NoteType.Don ? donPrefab : kaPrefab;
            GameObject noteObject = Instantiate(prefab, transform);
            noteObject.transform.localPosition = new Vector3((float)note.time * 30, 0, 0);
            noteObject.SetActive(true);
        }
    }
}
