using System.Collections;
using System.Reflection;
using UnityEngine;

namespace FactoryAssembly
{
    public class ResultBinderConverter : MonoBehaviour
    {
        private const string FRONT_SHEET_NAME = "Paper_letter/SheetFront";
        public Texture TargetTexture = null;

        public GameObject Invoice = null;

        internal bool Converted
        {
            get;
            private set;
        }

        private static readonly FieldInfo _displayRoutineField = null;
        private KMAudio _audio = null;

        static ResultBinderConverter()
        {
            _displayRoutineField = typeof(ResultPage).GetField("displayRoutine", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void Awake()
        {
            _audio = GetComponent<KMAudio>();
        }

        internal void Convert()
        {
            if (Converted)
            {
                return;
            }

            StartCoroutine(ConvertCoroutine());

            Converted = true;
            Invoice.SetActive(true);
        }

        internal void Revert()
        {
            Converted = false;
            Invoice.SetActive(false);
        }

        internal void ShowStamp()
        {
            BombBinder bombBinder = SceneManager.Instance.PostGameState.Room.BombBinder;
            ShowStamp(bombBinder.ResultDefusedPage);
            ShowStamp(bombBinder.ResultExplodedPage);
            ShowStamp(bombBinder.ResultFreeplayDefusedPage);
            ShowStamp(bombBinder.ResultFreeplayExplodedPage);
            ShowStamp(bombBinder.ResultTournamentPage);
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
            Transform sheetFront = resultPage.transform.Find(FRONT_SHEET_NAME);
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

        private void ShowStamp(ResultPage page)
        {
            if (page.gameObject.activeInHierarchy)
            {
                page.Stamp.SetActive(true);
                _audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Stamp, page.Stamp.transform);
                KTInputManager.Instance.AddInteractionPunch(page.Stamp.transform.position, 1.0f, 0.75f, 0.3f);
            }
        }
    }
}
