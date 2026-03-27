using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

namespace CookOrPanic.SocketController
{
    using CookOrPanic.Food;
    using CookOrPanic.ProcessedIngredient;


    [RequireComponent(typeof(XRSocketInteractor))]
    public class SocketController : MonoBehaviour
    {
        private XRSocketInteractor _socket;

        // Event yang akan didengarkan oleh CookingStation
        public event Action<ProcessedIngredient> OnIngredientEntered;
        public event Action<Food, GameObject> OnFoodEntered;
        public Action<GameObject> OnObjectEntered;
        public event Action OnObjectRemoved;

        private void Awake() => _socket = GetComponent<XRSocketInteractor>();

        private void OnEnable()
        {
            _socket.selectEntered.AddListener(HandleSelectEntered);
            _socket.selectExited.AddListener(HandleSelectExited);
        }

        private void OnDisable()
        {
            // Tambahkan pengecekan ini untuk mencegah NullReferenceException
            if (_socket != null)
            {
                _socket.selectEntered.RemoveListener(HandleSelectEntered);
                _socket.selectExited.RemoveListener(HandleSelectExited);
            }
        }

        // Tambahkan ini agar kita bisa akses list benda yang sedang menempel
        public List<IXRSelectInteractable> interactablesSelected => _socket.interactablesSelected;

        // Tambahkan ini agar kita bisa akses InteractionManager-nya
        public XRInteractionManager interactionManager => _socket.interactionManager;

        // Tambahkan ini juga jika kamu butuh akses ke komponen aslinya secara langsung
        public XRSocketInteractor Socket => _socket;

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            GameObject obj = args.interactableObject.transform.gameObject;

            // Cek apakah ini bahan mentah
            if (obj.TryGetComponent(out ProcessedIngredient ingredient))
            {
                OnIngredientEntered?.Invoke(ingredient);
            }
            // Cek apakah ini makanan jadi
            else if (obj.TryGetComponent(out Food food))
            {
                OnFoodEntered?.Invoke(food, obj);
            }

            OnObjectEntered?.Invoke(obj);
        }

        private void HandleSelectExited(SelectExitEventArgs args)
        {
            OnObjectRemoved?.Invoke();
        }


        // Fungsi pembantu jika ingin mengeluarkan benda secara paksa via code
        public void ForceEject()
        {
            if (_socket.hasSelection)
            {
                var interactable = _socket.interactablesSelected[0];
                _socket.interactionManager.SelectExit(_socket, interactable);
            }
        }

       
    }
}

