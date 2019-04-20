using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FactoryAssembly
{
    public class Calendar : MonoBehaviour
    {
        public float CurrentMonthAlpha = 1.0f;
        public float OtherMonthAlpha = 0.1f;

        public Color WeekdayImageColor = Color.white;
        public Color WeekdayTextColor = Color.white;

        public Color WeekendImageColor = Color.white;
        public Color WeekendTextColor = Color.white;

        public Image TopImage;
        public Text MonthText;
        public GridLayoutGroup CalendarGrid;
        public GameObject CalendarCellTemplate;

        private void Awake()
        {
            DateTime now = DateTime.Today;
            MonthText.text = now.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

            DateTime firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
            DayOfWeek firstOfMonthDay = firstOfMonth.DayOfWeek;

            int initialDayOffset = (int)firstOfMonth.DayOfWeek;

            for (DateTime cellDay = firstOfMonth - TimeSpan.FromDays(initialDayOffset); cellDay.DayOfWeek != DayOfWeek.Sunday || cellDay.Month <= now.Month || cellDay.Year < now.Year; cellDay += TimeSpan.FromDays(1.0))
            {
                GameObject cell = Instantiate(CalendarCellTemplate, CalendarGrid.transform);

                Image image = cell.GetComponentInChildren<Image>(true);
                Text text = cell.GetComponentInChildren<Text>(true);

                Color imageColor;
                Color textColor;

                switch (cellDay.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                        imageColor = WeekendImageColor;
                        textColor = WeekendTextColor;
                        break;

                    default:
                        imageColor = WeekdayImageColor;
                        textColor = WeekdayTextColor;
                        break;
                }

                imageColor.a = textColor.a = cellDay.Month == now.Month ? CurrentMonthAlpha : OtherMonthAlpha;

                image.color = imageColor;

                text.text = cellDay.Day.ToString();
                text.color = textColor;

                Transform circle = cell.transform.Find("Circle");
                if (circle != null && (cellDay.Day != now.Day || cellDay.Month != now.Month))
                {
                    DestroyImmediate(circle.gameObject);
                }

                cell.SetActive(true);
            }
        }
    }
}
