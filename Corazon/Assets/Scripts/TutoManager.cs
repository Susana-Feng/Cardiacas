using System;
using System.Collections;
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

            [SerializeField]
            public string buttonText2;
        }

        [SerializeField]
        public TextMeshProUGUI m_StepButtonTextField;
        [SerializeField]
        public TextMeshProUGUI m_StepButtonTextField2;

        [SerializeField]
        List<Step> m_StepList = new List<Step>();

        [Header("Second Button (shown on last step)")]
        [SerializeField]
        GameObject m_SecondButton;

        [Header("Object to deactivate on second button press")]
        [SerializeField]
        GameObject m_ObjectToDeactivate;

        [Header("Optional object to activate on second button press")]
        [SerializeField]
        GameObject[] m_ObjectsToActivate;

        [Header("Script opcional a inicializar tuto")]
        // Referencia al segundo script desde el Inspector
        public LanzadorTutorial lanzadorTutorial;

        [Header("Script opcional partida 30 seg")]
        // Referencia al segundo script desde el Inspector
        public LanzadorObjetos lanzadorObjetos;

        [Header("Script opcional timer 30 seg")]
        // Referencia al segundo script desde el Inspector
        public Timer timer;

        int m_CurrentStepIndex = 0;

        void Start()
        {
            Debug.Log("StepManager iniciado correctamente");

            bool isFirstStep = m_CurrentStepIndex == 0;
            SetSecondButtonVisible(!isFirstStep);

            // Aplica el texto del segundo botón si aplica
            if (!isFirstStep && m_StepButtonTextField2 != null)
                m_StepButtonTextField2.text = m_StepList[m_CurrentStepIndex].buttonText2;
        }

        public void Next()
        {
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);
            m_CurrentStepIndex = (m_CurrentStepIndex + 1) % m_StepList.Count;
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
            m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;

            if (m_StepButtonTextField2 != null)
                m_StepButtonTextField2.text = m_StepList[m_CurrentStepIndex].buttonText2;

            Debug.Log($"Step actual: {m_CurrentStepIndex} / {m_StepList.Count - 1}");

            bool isFirstStep = m_CurrentStepIndex == 0;
            SetSecondButtonVisible(!isFirstStep); // Visible en todos menos el primero
        }

        public void Previous()
        {
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);
            m_CurrentStepIndex = (m_CurrentStepIndex - 1 + m_StepList.Count) % m_StepList.Count;
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
            m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;
            m_StepButtonTextField2.text = m_StepList[m_CurrentStepIndex].buttonText2;

            Debug.Log($"Step actual: {m_CurrentStepIndex} / {m_StepList.Count - 1}");

            bool isFirstStep = m_CurrentStepIndex == 0;
            SetSecondButtonVisible(!isFirstStep);
        }

        public void ReiniciarTutorial()
        {
            // Ocultar el step actual
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);

            // Avanzar al siguiente índice
            m_CurrentStepIndex++;

            // Si todavía hay pasos disponibles
            if (m_CurrentStepIndex < m_StepList.Count)
            {
                m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
                m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;

                Debug.Log($"Step actual: {m_CurrentStepIndex} / {m_StepList.Count - 1}");

                bool isLastStep = m_CurrentStepIndex == m_StepList.Count - 1;
                SetSecondButtonVisible(isLastStep);
            }
            else
            {
                // Antes de desactivar, volver al primer card
                m_CurrentStepIndex = 0;
                m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
                m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;
                SetSecondButtonVisible(false);

                Debug.Log("Reiniciado al primer card antes de desactivar.");;

                // Ya se llegó al último card → desactivar el objeto principal y activar otro
                if (m_ObjectToDeactivate != null)
                {
                    deactivateObject();
                }

                if (m_ObjectsToActivate != null)
                {
                    activateObjects();
                }
            }
        }

        public void OnFirstButtonPressed()
        {
            SetSecondButtonVisible(false);
            Next();
        }

        public void OnSecondButtonPressed()
        {
            deactivateObject();

            activateObjects();
        }

        public void OnBotonPresionado1()
        {
            if (m_CurrentStepIndex == 0)
                Next();
            else if (m_CurrentStepIndex == m_StepList.Count - 1)
                Next();
            else
                Previous();
        }

        public void OnBotonPresionado2()
        {
            if (m_CurrentStepIndex == m_StepList.Count - 1)
                OnIniciarButtonPressed();
            else
                Next();
        }

        public void OnIniciarButtonPressed()
        {
            Debug.Log("Iniciar button pressed - iniciando partida");

            deactivateObject();

            // Verificamos que el segundo script esté asignado
            if (lanzadorTutorial != null)
            {
                Debug.Log("Iniciando tutorial");
                ResetToFirstCard();
                lanzadorTutorial.Relanzar(); // Llamada al método público del segundo script
            }
            else if (lanzadorObjetos != null)
            {
                Debug.Log("Iniciando partida de 30 segundos");

                timer.IniciarContador(30f); // Iniciar el contador de 30 segundos);
                lanzadorObjetos.Relanzar(30f); // Llamada al método público del segundo script
                StartCoroutine(ActivarDespues(30f));
            }
            else
            {
                Debug.LogWarning("No se asignó el SegundoScript en el Inspector.");
            }
        }

        public void OnIniciarPartidaRealPressed()
        {
            deactivateObject();
            if (lanzadorObjetos != null)
            {
                Debug.Log("Iniciando partida de 60 segundos");
                timer.IniciarContador(60f); // Iniciar el contador de 60 segundos);
                lanzadorObjetos.Relanzar(60f); // Llamada al método público del segundo script
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

        private IEnumerator ActivarDespues(float tiempo)
        {
            yield return new WaitForSeconds(tiempo);
            activateObjects();
        }

        void activateObjects()
        {
            if (m_ObjectsToActivate != null && m_ObjectsToActivate.Length > 0)
            {
                foreach (GameObject obj in m_ObjectsToActivate)
                {
                    if (obj != null)
                    {
                        // Activar el GameObject
                        obj.SetActive(true);
                        Debug.Log($"Objeto activado: {obj.name}");

                        // Activar SkinnedMeshRenderer si existe
                        SkinnedMeshRenderer smr = obj.GetComponent<SkinnedMeshRenderer>();
                        if (smr != null)
                        {
                            smr.enabled = true;
                            Debug.Log($"SkinnedMeshRenderer activado en: {obj.name}");
                        }

                        // Buscar hijo llamado "CoachingCardRoot" y activarlo
                        Transform child = obj.transform.Find("CoachingCardRoot");
                        if (child != null)
                        {
                            child.gameObject.SetActive(true);
                            Debug.Log("Objeto hijo 'CoachingCardRoot' activado.");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("No se asignaron objetos en la lista m_ObjectsToActivate.");
            }
        }

        void deactivateObject()
        {
            if (m_ObjectToDeactivate != null)
            {
                // Desactivar el SkinnedMeshRenderer (si existe)
                SkinnedMeshRenderer smr = m_ObjectToDeactivate.GetComponent<SkinnedMeshRenderer>();
                if (smr != null)
                {
                    smr.enabled = false;
                    Debug.Log($"SkinnedMeshRenderer desactivado en: {m_ObjectToDeactivate.name}");
                }
                else
                {
                    Debug.LogWarning($"No se encontró SkinnedMeshRenderer en: {m_ObjectToDeactivate.name}");
                }

                // Buscar hijo llamado "CoachingCardRoot" y desactivarlo
                Transform child = m_ObjectToDeactivate.transform.Find("CoachingCardRoot");
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                    Debug.Log("Objeto hijo 'CoachingCardRoot' desactivado.");
                }
                else
                {
                    Debug.LogWarning("No se encontró un hijo llamado 'CoachingCardRoot'.");
                }
            }
            else
            {
                Debug.LogWarning("m_ObjectToDeactivate no está asignado.");
            }
        }

        public void ResetToFirstCard()
        {
            // Ocultar el card actual
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);

            // Reiniciar índice al primer card
            m_CurrentStepIndex = 0;

            // Mostrar el primer card
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
            m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;

            // Ocultar el segundo botón
            SetSecondButtonVisible(false);

            Debug.Log("Tutorial reiniciado al primer card.");
        }


    }

}


