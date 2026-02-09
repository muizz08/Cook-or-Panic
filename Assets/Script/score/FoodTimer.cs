using UnityEngine;
using TMPro;
using System.Collections;



namespace CookOrPanic.FoodTimer
{
    public class FoodTimer : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;

        private int seconds;
        private Coroutine cookingRoutine;

        public void StartCooking()
        {
            StopCooking(); // jaga-jaga

            seconds = 0;
            UpdateText();
            timerText.gameObject.SetActive(true);

            cookingRoutine = StartCoroutine(TimerRoutine());
        }

        public void StopCooking()
        {
            if (cookingRoutine != null)
            {
                StopCoroutine(cookingRoutine);
                cookingRoutine = null;
            }

            timerText.gameObject.SetActive(false);
        }

        private IEnumerator TimerRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                seconds++;
                UpdateText();
            }

        }

        private void UpdateText()
        {
            timerText.text = seconds + "s";
        }

    
    }

}
