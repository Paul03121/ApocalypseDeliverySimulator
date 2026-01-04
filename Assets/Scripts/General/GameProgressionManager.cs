using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameProgressionManager : MonoBehaviour
{
    public static GameProgressionManager Instance;

    [Header("References")]
    [SerializeField] private DeliveryManager deliveryManager;
    [SerializeField] private Inventory playerInventory;

    [Header("UI")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 2f;

    private int pendingMissions = 0;
    private int lastPendingMissions = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        DeliveryManager.OnDeliveryCompleted += HandleMissionCompleted;
    }

    private void OnDisable()
    {
        DeliveryManager.OnDeliveryCompleted -= HandleMissionCompleted;
    }

    void Start()
    {
        Invoke(nameof(StartGame), 1f);
    }

    public void StartGame()
    {
        DeliveryFlags.Instance.SetFlag(DeliveryFlag.TutorialStep1);
        StartCoroutine(TutorialStep1Flow());
    }

    IEnumerator TutorialStep1Flow()
    {
        MessageUIManager.Instance.ShowMessage("Bienvenido al trabajo de delivery. Puede que el mundo se haya acabado, pero alguien tiene que llevar paquetes.\n" +
            "Puedes moverte con WASD, saltar con la barra espaciadora, correr con SHIFT y agacharte con CTRL. También puedes cambiar tu perspectiva con la tecla V.\n" +
            "Ya tienes tu primera misión asignada. Abre el mapa con M, busca el ícono de la caja con marco azul para recoger el paquete y entrégalo donde veas la caja con marco naranja.\n" +
            "También hay una opción en la que puedes marcar rutas en el mapa para saber a dónde ir. Y por último... ¡ten cuidado con los zombies!");

        deliveryManager.GenerateMissions();
        UpdatePendingMissions();

        yield return new WaitUntil(() => pendingMissions == 0);

        MessageUIManager.Instance.ShowMessage("Buen trabajo con tu primera entrega, no todos duran tanto. Ahora te recomiendo gastar ese dinero con el mayor vendedor de la ciudad.\n" +
            "Lo encontrarás en el mapa con el ícono de tienda con borde amarillo. Necesitarás comprar algo para seguir con los siguientes trabajos");

        yield return new WaitUntil(() => playerInventory.HasAnyItem());

        DeliveryFlags.Instance.SetFlag(DeliveryFlag.TutorialStep2);
        StartCoroutine(TutorialStep2Flow());
    }

    IEnumerator TutorialStep2Flow()
    {
        MessageUIManager.Instance.ShowMessage("Excelente elección. Si quieres, puedes utilizar o equipar lo que compraste desde tu inventario, al que puedes acceder con la tecla TAB.\n" +
            "Ahora hay cuatro entregas nuevas disponibles, aunque en algunas tendrás que transportar paquetes especiales. Pero no te preocupes, los clientes te darán las instrucciones cuando llegues.");

        deliveryManager.GenerateMissions();
        UpdatePendingMissions();

        yield return new WaitUntil(() => pendingMissions == 0);

        DeliveryFlags.Instance.SetFlag(DeliveryFlag.TutorialStep3);
        StartCoroutine(TutorialStep3Flow());
    }

    IEnumerator TutorialStep3Flow()
    {
        MessageUIManager.Instance.ShowMessage("Excelente trabajo. Que bueno que te adaptaste rápido, porque hay dos entregas nuevas esperándote.\n" +
            "Como me caes bien, te voy a contar un secreto. Corre el rumor de que el vendedor suele dejar un par de armas en el DINER donde tiene su negocio, quizá por descuido, quizá a propósito.\n" +
            "Tal vez si entras por la puerta trasera y miras con atención encuentres algo interesante. O tal vez no. En esta ciudad nunca se sabe... tú decides.\n" +
            "Si al final decides ir a comprobar si el rumor es cierto, puedes atacar con un arma pulsando el clic izquierdo, guardarlas con la tecla F y soltarlas con la tecla Q.");

        deliveryManager.GenerateMissions();
        UpdatePendingMissions();

        yield return new WaitUntil(() => pendingMissions == 0);

        DeliveryFlags.Instance.SetFlag(DeliveryFlag.TutorialStep4);
        StartCoroutine(TutorialStep4Flow());
    }

    IEnumerator TutorialStep4Flow()
    {
        MessageUIManager.Instance.ShowMessage("Buen trabajo con esas entregas. Sé que no ha sido un día fácil ahí afuera, pero ya solo queda un último encargo.\n" +
            "La persona que lo solicitó fue extremadamente seria, de esas que no hacen preguntas ni dan explicaciones. Ve con cuidado, mantente alerta y no bajes la guardia en ningún momento.");

        deliveryManager.GenerateForcedMission(DeliveryFlag.TutorialStep4);
        UpdatePendingMissions();

        yield return new WaitUntil(() => pendingMissions == 0);

        yield return new WaitUntil(() => GameStateManager.Instance.IsGameplay);

        MessageUIManager.Instance.ShowMessage("Demo completada. ¡Gracias por jugar!");

        yield return new WaitForSecondsRealtime(6f);

        yield return FadeOut();

        yield return new WaitForSecondsRealtime(3f);

        ReturnToMainMenu();
    }

    private void HandleMissionCompleted()
    {
        UpdatePendingMissions();

        if (pendingMissions != lastPendingMissions)
        {
            if (pendingMissions == 3)
                MessageUIManager.Instance.ShowMessage("Solo te faltan 3 entregas. ¡Sigue así!");
            else if (pendingMissions == 2)
                MessageUIManager.Instance.ShowMessage("Un par de entregas más y listo");
            else if (pendingMissions == 1)
                MessageUIManager.Instance.ShowMessage("Solo te falta 1 entrega. ¡Ánimo!");
        }

        lastPendingMissions = pendingMissions;
    }

    private void UpdatePendingMissions()
    {
        pendingMissions = DeliveryManager.Instance.GeneratedMissions.Count;
    }

    private IEnumerator FadeOut()
    {
        if (fadeCanvas == null)
            yield break;

        fadeCanvas.gameObject.SetActive(true);
        fadeCanvas.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvas.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 1f;
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
