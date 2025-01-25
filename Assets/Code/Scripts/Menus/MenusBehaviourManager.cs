using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public enum MenuType { MainMenu, PauseMenu }

public class MenusBehaviourManager : MonoBehaviour
{
    #region Variables

    [Header("Enums Variables")]
    public MenuType _menuType;
    public InputType localInputType;
    public InputType previousInputType;
    
    [Header("Input")]
    private PlayerInput playerInput;
    private bool mouseState;
    private bool mouseByPass;
    private bool isMouseVisible;
    private float inputSwitchCooldown = 0.5f;
    private float lastInputSwitchTime;
    

    [Header("Menus, Animators y Botones")]
    public List<GameObject> Menus;
    private List<Animator> MenusAnimators = new List<Animator>();
    private List<List<GameObject>> ButtonsMenus = new List<List<GameObject>>();
    private Dictionary<string, int> menuIndexLookup;
    private Dictionary<GameObject, GameObject> lastSelectedButtons = new Dictionary<GameObject, GameObject>();

    #endregion

    #region Running Instances
    private void Awake()
    {
        playerInput = FindObjectOfType<PlayerInput>(); // Buscar PlayerInput y asignarlo.
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component not found.");
        }

        menuIndexLookup = new Dictionary<string, int>(); // Inicializar el diccionario para evitar NullReferenceException
        MenusAnimators.Clear(); // Asegúrate de limpiar la lista antes de llenarla
        ButtonsMenus.Clear();   // Asegúrate de limpiar la lista antes de llenarla

        for (int i = 0; i < Menus.Count; i++)
        {
            GameObject menu = Menus[i];

            if (menu == null)
            {
                Debug.LogError($"Menu at index {i} is null.");
                continue;
            }

            Animator menuAnimator = menu.GetComponent<Animator>(); // Obtener y agregar el Animator si existe
            if (menuAnimator != null)
            {
                MenusAnimators.Add(menuAnimator);
            }

            List<GameObject> menuButtons = new List<GameObject>(); // Obtener todos los botones y sliders en el menú en el orden de la jerarquía
 
            Button[] buttons = menu.GetComponentsInChildren<Button>(true); // Obtén todos los botones en el menú en el orden en que están en la jerarquía
            foreach (Button button in buttons)
            {
                menuButtons.Add(button.gameObject);
            }

            Slider[] sliders = menu.GetComponentsInChildren<Slider>(true); // Obtén todos los sliders en el menú en el orden en que están en la jerarquía
            foreach (Slider slider in sliders)
            {
                menuButtons.Add(slider.gameObject);
            }

            ButtonsMenus.Add(menuButtons);
            menuIndexLookup[menu.name] = i;
        }
    }

    private void Start()
    {
        LoadInputTypeFromGameManager(); // Cargar tipo de input desde el GameManager.
        ConfigureInputMode();
        Close_Charge();
        
        if(localInputType == InputType.Keyboard)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isMouseVisible = false;
        }
        else if(localInputType == InputType.Mouse)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isMouseVisible = true;
        }

        GameManager.Instance.OnInputTypeChanged += UpdateInputType;
    }

    private void Update()
    {
        HandlePause(); // Manejar pausas

        if (GameManager.Instance.CurrentGameState == GameStates.Pause || _menuType == MenuType.MainMenu)
        {
            mouseState = IsMouseOnScreen(); // Detectar si el mouse está en la pantalla

            if (localInputType == InputType.Automatic)
            {
                DetectInputDevice(); // Detectar dispositivo activo
            }

            EnsureButtonSelected(); // Asegurar selección de botón
        }

        if (localInputType != previousInputType)
        {
            ConfigureInputMode(); // Configurar comportamiento según el nuevo modo
            previousInputType = localInputType;
        }
    }

    private void OnGUI()
    {
        if (!isMouseVisible)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    #endregion

    #region Pause Management
    private void HandlePause()
    {
        if (InputManager.GetInstance().GetPausePressed() && _menuType == MenuType.PauseMenu)
        {
            if (GameManager.Instance.CurrentGameState == GameStates.Pause)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        GameManager.Instance.CurrentGameState = GameStates.Pause;
        Menus[0].SetActive(true);

        EnsureButtonSelected(); // Asegurarse de que haya un botón seleccionado

        playerInput.SwitchCurrentActionMap("Pause");
    }

    public void ResumeGame()
    {
        GameManager.Instance.CurrentGameState = GameStates.Gameplay;

        Close_Charge();

        playerInput.SwitchCurrentActionMap("Gameplay");
    }

    #endregion

    #region Input Management

    private void ConfigureInputMode()
    {
        switch (localInputType)
        {
            case InputType.Mouse:
                SetControlScheme("Mouse");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                isMouseVisible = true;
                EventSystem.current.SetSelectedGameObject(null); // Desseleccionar cualquier objeto para evitar conflictos.
                break;

            case InputType.Keyboard:
                SetControlScheme("Keyboard");
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                isMouseVisible = false;
                EnsureButtonSelected(); // Seleccionar el primer botón visible en el menú activo.
                break;

            case InputType.MouseKeyboard:
                SetControlScheme("MouseKeyboard");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                isMouseVisible = true;
                EnsureButtonSelected(); // Seleccionar el primer botón visible en el menú activo.
                break;

            case InputType.Gamepad:
                SetControlScheme("Gamepad");
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                isMouseVisible = false;
                EnsureButtonSelected(); // Seleccionar el primer botón visible en el menú activo.
                break;

            case InputType.Automatic:
                SetControlScheme("Automatic");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                isMouseVisible = true;
                EnsureButtonSelected(); // Mantén la detección automática activa.
                break;
        }
    }
    private bool IsMouseOnScreen()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        return mousePosition.x >= 0 && mousePosition.x <= Screen.width &&
            mousePosition.y >= 0 && mousePosition.y <= Screen.height;
    }

    private void DetectInputDevice()
    {
        if (Time.time - lastInputSwitchTime < inputSwitchCooldown)
            return;

        if (Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            SwitchInputType(InputType.Mouse);
        }
        else if (Keyboard.current.anyKey.wasPressedThisFrame && Mouse.current.delta.ReadValue() == Vector2.zero)
        {
            SwitchInputType(InputType.Keyboard);
        }
        else if (Keyboard.current.anyKey.wasPressedThisFrame && Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            SwitchInputType(InputType.MouseKeyboard);
        }
        else if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            SwitchInputType(InputType.Gamepad);
        }
    }


    private void SwitchInputType(InputType newType)
    {
        if (localInputType != newType)
        {
            localInputType = newType;
            ConfigureInputMode(); // Configurar el modo de entrada
            SaveInputTypeToGameManager(); // Guardar en el GameManager
            lastInputSwitchTime = Time.time;
        }
    }

    private void SetControlScheme(string controlScheme)
    {
        try
        {
            playerInput.SwitchCurrentControlScheme(controlScheme);
            Debug.Log($"Switched to control scheme: {controlScheme}");
        }
        catch
        {
            Debug.LogError($"Control Scheme '{controlScheme}' not found or not configured.");
        }
    }

    #region Persistence Input Data

    private void LoadInputTypeFromGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CurrentInputType = localInputType; // Cargar desde el GameManager
            Debug.Log($"Loaded InputType: {localInputType}");
        }
    }

    private void SaveInputTypeToGameManager()
    {
        if (GameManager.Instance != null)
        {
            localInputType = GameManager.Instance.CurrentInputType; // Guardar en el GameManager
            Debug.Log($"Saved InputType: {localInputType}");
        }
    }
    public void SetInputType(InputType newInputType)
    {
        Debug.Log($"Setting InputType from SettingsMenu: {newInputType}");
        SwitchInputType(newInputType); // Cambia el modo y guarda la configuración
    }

    private void UpdateInputType(InputType newInputType)
    {
        localInputType = newInputType;
        ConfigureInputMode();
        Debug.Log($"MenusBehaviourManager updated to InputType: {localInputType}");
    }

    #endregion
    #endregion

    #region Menus Management
    #region Functions Transitions (In Buttons)

    public void MenuTransition(GameObject MenuActivate)
    {
        CleanLastSelectedButtons(); // Limpiar botones obsoletos al hacer la transición.

        // Obtener el botón que activó la transición
        GameObject currentButton = GetCurrentButton();
        if (currentButton == null)
        {
            Debug.Log("No button selected.");
            return;
        }

        // Encontrar el MenuDeactivate correspondiente basado en el botón actual
        GameObject MenuDeactivate = FindMenuDeactivateFromButton(currentButton);

        if (MenuDeactivate != null)
        {
            GameObject lastButton = EventSystem.current.currentSelectedGameObject;
            if (lastButton != null)
            {
                lastSelectedButtons[MenuDeactivate] = lastButton;
                Debug.Log($"Saved last selected button: {lastButton.name}");
            }
        }

        // Obtener los animadores para ambos menús
        Animator MenuA = null;
        Animator MenuB = null;

        int MenuAIndex;
        if (menuIndexLookup.TryGetValue(MenuActivate.name, out MenuAIndex))
        {
            if (MenuAIndex >= 0 && MenuAIndex < MenusAnimators.Count)
            {
                MenuA = MenusAnimators[MenuAIndex];
            }
            else
            {
                Debug.Log($"MenuAIndex {MenuAIndex} is out of range.");
            }
        }
        else
        {
            Debug.Log($"MenuActivate with name {MenuActivate.name} not found in menuIndexLookup.");
        }

        int MenuBIndex;
        if (menuIndexLookup.TryGetValue(MenuDeactivate.name, out MenuBIndex))
        {
            if (MenuBIndex >= 0 && MenuBIndex < MenusAnimators.Count)
            {
                MenuB = MenusAnimators[MenuBIndex];
            }
            else
            {
                Debug.Log($"MenuBIndex {MenuBIndex} is out of range.");
            }
        }
        else    
        {
            Debug.Log($"MenuDeactivate with name {MenuDeactivate.name} not found in menuIndexLookup.");
        }

        // Llamar a la corrutina de transición con animaciones si se encuentran animadores
        if (MenuA != null || MenuB != null)
        {
            StartCoroutine(AnimationTransition(MenuActivate, MenuDeactivate, MenuA, MenuB));
        }
        else
        {
            StartCoroutine(DOTweenTransition(MenuActivate, MenuDeactivate));
        }
    }
    #endregion  

    #region Behaviour Transitions
    public void Close_Charge()
    {
        foreach (GameObject menu in Menus)
        {
            menu.SetActive(true);
            menu.SetActive(false);
        }

        if(_menuType == MenuType.MainMenu)
        {
            MenuIgnition();
            GameManager.Instance.CurrentGameState = GameStates.Pause;
        }
        else
        {
            GameManager.Instance.CurrentGameState = GameStates.Gameplay;
        }
    }
    private void MenuIgnition()
    {
        GameObject IgnitedMenu = Menus[0];
        Animator MenuA = null;
        int MenuAIndex;

        if (menuIndexLookup.TryGetValue(IgnitedMenu.name, out MenuAIndex))
        {
            if (MenuAIndex >= 0 && MenuAIndex < MenusAnimators.Count)
            {
                MenuA = MenusAnimators[MenuAIndex];
            }
            else
            {
                Debug.Log($"MenuAIndex {MenuAIndex} is out of range.");
            }
        }
        else
        {
            Debug.Log($"MenuActivate with name {IgnitedMenu.name} not found in menuIndexLookup.");
        }

        if (MenuA != null)
        {
            StartCoroutine(AnimationIgnition(IgnitedMenu, MenuA));
        }
        else
        {
            StartCoroutine(DOTweenIgnition(IgnitedMenu));
        }
    }

    private GameObject GetFirstButton(GameObject menu)
    {
        // Encuentra el primer botón hijo del menú activado
        Button[] buttons = menu.GetComponentsInChildren<Button>();
        if (buttons.Length > 0)
        {
            return buttons[0].gameObject;
        }
        return null;
    }

    private GameObject GetCurrentButton()
    {
        // 1. Verificar si hay un botón seleccionado actualmente.
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            return EventSystem.current.currentSelectedGameObject;
        }

        // 2. Detectar el botón bajo el mouse (solo si el esquema actual permite usar el mouse).
        if (localInputType == InputType.Mouse || localInputType == InputType.Automatic)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.GetComponent<Button>() != null || result.gameObject.GetComponent<Slider>() != null)
                {
                    return result.gameObject; // Retornar el primer botón o slider encontrado bajo el mouse.
                }
            }
        }

        // 3. Detectar el botón mediante navegación con teclado o gamepad.
        if (localInputType == InputType.Keyboard || localInputType == InputType.Gamepad || localInputType == InputType.MouseKeyboard)
        {
            // En caso de que no haya un botón seleccionado, seleccionar automáticamente el primero.
            foreach (var menu in Menus)
            {
                if (menu.activeSelf && menuIndexLookup.TryGetValue(menu.name, out int menuIndex))
                {
                    if (menuIndex < ButtonsMenus.Count && ButtonsMenus[menuIndex].Count > 0)
                    {
                        return ButtonsMenus[menuIndex][0]; // Retornar el primer botón del menú activo.
                    }
                }
            }
        }

        // 4. Si no se encuentra ningún botón, retornar null.
        Debug.LogWarning("No button found with the current input method.");
        return null;
    }


    private GameObject FindMenuDeactivateFromButton(GameObject button)
    {
        foreach (var menu in Menus)
        {
            if (menuIndexLookup.TryGetValue(menu.name, out int menuIndex) && menuIndex < ButtonsMenus.Count)
            {
                List<GameObject> buttons = ButtonsMenus[menuIndex];
                if (buttons.Contains(button))
                {
                    Debug.Log($"Menu to deactivate found: {menu.name}");
                    return menu;
                }
            }
        }

        Debug.Log("MenuDeactivate could not be found.");
        return null;
    }

    private void SelectFirstButtonInActiveMenu()
    {
        foreach (GameObject menu in Menus)
        {
            int menuIndex;
            if (menu.activeSelf && menuIndexLookup.TryGetValue(menu.name, out menuIndex))
            {
                if (menuIndex < ButtonsMenus.Count)
                {
                    List<GameObject> menuButtons = ButtonsMenus[menuIndex];
                    if (menuButtons.Count > 0)
                    {
                        GameObject firstButton = menuButtons[0];
                        EventSystem.current.SetSelectedGameObject(null);
                        EventSystem.current.SetSelectedGameObject(firstButton);
                        Debug.Log($"First button selected: {firstButton.name}");
                        return;
                    }
                }
            }
        }

        // Fallback en caso de no encontrar botones
        EventSystem.current.SetSelectedGameObject(null);
        Debug.LogWarning("No active menu or buttons found to select.");
    }

    private void CleanLastSelectedButtons()
    {
        foreach (var menu in new List<GameObject>(lastSelectedButtons.Keys))
        {
            if (!Menus.Contains(menu))
            {
                lastSelectedButtons.Remove(menu);
            }
        }
    }

    private void EnsureButtonSelected()
    {
        // En modo Mouse, deseleccionamos cualquier botón.
        if (localInputType == InputType.Mouse)
        {
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        // En modo Automático, si el mouse está en la pantalla, deseleccionamos.
        if (localInputType == InputType.Automatic && IsMouseOnScreen())
        {
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        // Validar si ya hay un botón seleccionado y activo.
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null && selected.activeInHierarchy)
        {
            return; // Si hay un botón seleccionado válido, no hacemos nada.
        }

        // Seleccionar el primer botón del menú activo.
        foreach (GameObject menu in Menus)
        {
            if (menu.activeSelf)
            {
                SelectFirstButtonInMenu(menu);
                break;
            }
        }
    }

    private void SelectFirstButtonInMenu(GameObject menu)
    {
        int menuIndex;
        if (!menuIndexLookup.TryGetValue(menu.name, out menuIndex) || menuIndex >= ButtonsMenus.Count)
        {
            Debug.LogWarning($"Menu {menu.name} no tiene botones o no se encontró en el índice.");
            return;
        }

        List<GameObject> buttons = ButtonsMenus[menuIndex];
        if (buttons.Count == 0)
        {
            Debug.LogWarning($"Menu {menu.name} no tiene botones activos.");
            return;
        }

        // Seleccionar el primer botón
        GameObject firstButton = buttons[0];
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButton);
        Debug.Log($"Selected first button: {firstButton.name}");
    }

    #region Animation Management
    private IEnumerator AnimationIgnition(GameObject MenuActivate, Animator MenuA)
    {
        MenuActivate.SetActive(true);

        if (MenuA != null)
        {
            MenuA.SetTrigger("Open");
        }

        yield return new WaitUntil(() =>
            (MenuA == null || MenuA.GetCurrentAnimatorStateInfo(0).IsName("Open"))
        );

        playerInput.actions.Disable();

        yield return new WaitUntil(() =>
            (MenuA == null || MenuA.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        );

        if(localInputType != InputType.Mouse)
        {
            GameObject firstButton = GetFirstButton(MenuActivate);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }

        playerInput.actions.Enable();
    }

    private IEnumerator AnimationTransition(GameObject MenuActivate, GameObject MenuDeactivate, Animator MenuA, Animator MenuB)
    {
        MenuActivate.SetActive(true);

        if (MenuA != null)
        {
            MenuA.SetTrigger("Open");
        }

        if (MenuB != null)
        {
            MenuB.SetTrigger("Close");
        }

        yield return new WaitUntil(() =>
            MenuA == null || MenuA.GetCurrentAnimatorStateInfo(0).IsName("Open") ||
            MenuB == null || MenuB.GetCurrentAnimatorStateInfo(0).IsName("Close")
        );

        playerInput.actions.Disable();

        yield return new WaitUntil(() =>
            (MenuA == null || MenuA.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f) &&
            (MenuB == null || MenuB.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        );

        MenuDeactivate.SetActive(false);

        // Obtener el primer botón del MenuActivate y seleccionarlo
        if(localInputType != InputType.Mouse)
        {
            GameObject firstButton = GetFirstButton(MenuActivate);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
            print("Botón seleccionado");
        }
        
        playerInput.actions.Enable();
    }
    
    private IEnumerator DOTweenIgnition(GameObject MenuActivate)
    {
        playerInput.actions.Disable();

        // Activar el menú
        MenuActivate.SetActive(true);

        // Configura una animación simple con DOTween (si es necesario)
        MenuActivate.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad);

        yield return new WaitForSeconds(0.5f);

        // Seleccionar el primer botón en el menú activado
        if (localInputType != InputType.Mouse)
        {
            GameObject firstButton = GetFirstButton(MenuActivate);
            if (firstButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstButton);
                Debug.Log($"Selected first button: {firstButton.name}");
            }
            else
            {
                Debug.Log("No buttons found in the active menu.");
            }
        }

        playerInput.actions.Enable();
    }

    private IEnumerator DOTweenTransition(GameObject MenuActivate, GameObject MenuDeactivate)
    {
        playerInput.actions.Disable();

        // Activar y desactivar menús con animaciones de escala
        MenuActivate.SetActive(true);
        MenuActivate.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad);

        if (MenuDeactivate != null)
        {
            MenuDeactivate.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
            yield return new WaitForSeconds(0.5f);
            MenuDeactivate.SetActive(false);
        }

        // Seleccionar el primer botón del menú activado
        if (localInputType != InputType.Mouse)
        {
            SelectFirstButtonInActiveMenu();
        }

        playerInput.actions.Enable();
    }

    #endregion
    #endregion

    #region Scene Management

    public void ReturnToMenu()
    {
    #if UNITY_WEBGL
            SceneManager.LoadScene("MainMenu_WEB");
    #elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            SceneManager.LoadScene("MainMenu_PC");
    #endif
    }

    public void ChangeScene(string n)
    {
        SceneManager.LoadScene(n);
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    public void RestartLevel()
    {
        int actualScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(actualScene);
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        #if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            Application.Quit(); 
        #endif
    }

    #endregion
    #endregion

    #region OnFunctions
    private void OnDestroy()
    {
        // Desuscribirse del evento al destruirse el objeto
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnInputTypeChanged -= UpdateInputType;
        }
    }

    #endregion

    #region Audio Functions
    /*public void ReproducirPointerEnter()
    {
        if (isMouseVisible)
            AudioManager.Instance.PlaySfx(FMODEvents.instance.ButtonSelect);
    }
    public void ReproducirSelect()
    {
        if (!isMouseVisible)
            AudioManager.Instance.PlaySfx(FMODEvents.instance.ButtonSelect);
    }
    public void ReproducirSubmit()
    {
        if (!isMouseVisible)
            AudioManager.Instance.PlaySfx(FMODEvents.instance.ButtonSubmit);
    }
    public void ReproducirPointerClick()
    {
        if (isMouseVisible)
            AudioManager.Instance.PlaySfx(FMODEvents.instance.ButtonSubmit);
    }*/
    #endregion

}