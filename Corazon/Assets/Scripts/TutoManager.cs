using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.VRTemplate
{
    /// <summary>
    /// Controls the steps in the coaching card.
    /// </summary>
    public class TutoManager : MonoBehaviour
    {
        [Serializable]
        class Step
        {
            [SerializeField]
            public GameObject stepObject;

            [SerializeField]
            public string buttonText;
        }

        [SerializeField]
        public TextMeshProUGUI m_StepButtonTextField;

        [SerializeField]
        List<Step> m_StepList = new List<Step>();

        [Header("Second Button (shown on last step)")]
        [SerializeField]
        GameObject m_SecondButton;

        [Header("Object to destroy on second button press")]
        [SerializeField]
        GameObject m_ObjectToDestroy;

        [Header("Optional object to activate on second button press")]
        [SerializeField]
        GameObject[] m_ObjectsToActivate;

        [Header("Script opcional a inicializar")]
        // Referencia al segundo script desde el Inspector
        public LanzadorTutorial lanzadorTutorial;

        int m_CurrentStepIndex = 0;

        void Start()
        {
            Debug.Log("StepManager iniciado correctamente");

            if (m_SecondButton != null)
                m_SecondButton.SetActive(false);
        }

        public void Next()
        {
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);
            m_CurrentStepIndex = (m_CurrentStepIndex + 1) % m_StepList.Count;
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
            m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;

            Debug.Log($"Step actual: {m_CurrentStepIndex} / {m_StepList.Count - 1}");

            bool isLastStep = m_CurrentStepIndex == m_StepList.Count - 1;
            SetSecondButtonVisible(isLastStep);
        }

        public void OnFirstButtonPressed()
        {
            SetSecondButtonVisible(false);
            Next();
        }

        public void OnSecondButtonPressed()
        {
            if (m_ObjectToDestroy != null)
            {
                Destroy(m_ObjectToDestroy);
                Debug.Log($"Objeto destruido: {m_ObjectToDestroy.name}");
            }
            else
            {
                Debug.LogWarning("m_ObjectToDestroy no está asignado.");
            }

            if (m_ObjectsToActivate != null && m_ObjectsToActivate.Length > 0)
            {
                foreach (GameObject obj in m_ObjectsToActivate)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log($"Objeto activado: {obj.name}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("No se asignaron objetos en la lista m_ObjectsToActivate.");
            }
        }

        public void OnIniciarButtonPressed()
        {
            Debug.Log("Iniciar button pressed - iniciando partida de prueba");

            // Verificamos que el segundo script esté asignado
            if (lanzadorTutorial != null)
            {
                lanzadorTutorial.Relanzar(); // Llamada al método público del segundo script
            }
            else
            {
                Debug.LogWarning("No se asignó el SegundoScript en el Inspector.");
            }
        }

        void SetSecondButtonVisible(bool visible)
        {
            if (m_SecondButton == null)
            {
                Debug.LogWarning("m_SecondButton no está asignado.");
                return;
            }

            m_SecondButton.SetActive(visible);
            Debug.Log($"Segundo botón SetActive: {visible}");
        }
    }
}


