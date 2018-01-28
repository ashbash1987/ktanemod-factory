using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FactoryAssembly
{
    public class ResultBinderConverter : MonoBehaviour
    {
        public Texture TargetTexture = null;

        public bool Converted
        {
            get;
            private set;
        }

        private static readonly FieldInfo _displayRoutineField = null;

        static ResultBinderConverter()
        {
            _displayRoutineField = typeof(ResultPage).GetField("displayRoutine", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void Convert()
        {
            if (Converted)
            {
                return;
            }

            StartCoroutine(ConvertCoroutine());

            Converted = true;
        }

        public void Revert()
        {
            Converted = false;
        }

        private IEnumerator ConvertCoroutine()
        {
            yield return null;
            yield return null;

            BombBinder bombBinder = SceneManager.Instance.PostGameState.Room.BombBinder;
            ConvertResultsPage(bombBinder.ResultDefusedPage);
            ConvertResultsPage(bombBinder.ResultExplodedPage);
            ConvertResultsPage(bombBinder.ResultFreeplayDefusedPage);
            ConvertResultsPage(bombBinder.ResultFreeplayExplodedPage);
            ConvertResultsPage(bombBinder.ResultTournamentPage);
        }

        private void ConvertResultsPage(ResultPage resultPage)
        {
            Transform sheetFront = resultPage.transform.Find("Paper_letter/SheetFront");
            if (sheetFront != null)
            {
                MeshRenderer sheetFrontRenderer = sheetFront.GetComponent<MeshRenderer>();
                if (sheetFrontRenderer != null)
                {
                    sheetFrontRenderer.material.mainTexture = TargetTexture;
                }
            }

            Coroutine displayRoutine = (Coroutine)_displayRoutineField.GetValue(resultPage);
            if (displayRoutine != null)
            {
                resultPage.StopCoroutine(displayRoutine);
            }
            _displayRoutineField.SetValue(resultPage, null);

            TMPro.TextMeshPro[] textEntries = resultPage.GetComponentsInChildren<TMPro.TextMeshPro>(true);
            foreach (TMPro.TextMeshPro textEntry in textEntries)
            {
                if (textEntry.transform.parent.GetComponent<Selectable>() == null)
                {
                    textEntry.gameObject.SetActive(false);
                }
            }
        }
    }
}
