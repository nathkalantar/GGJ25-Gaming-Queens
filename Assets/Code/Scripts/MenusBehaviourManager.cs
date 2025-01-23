using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public enum MenuType{MainMenu, PauseMenu}
public enum InputType{Keyboard, Mouse, Automatic}
public class MenusBehaviourManager : MonoBehaviour
{
    [Header("Settings")]
    public MenuType _menuType;
    public InputType _inputType;
    
    [Header("Input")]
    private PlayerInput playerInput;
    private bool mouseState;
    private bool mouseByPass;
    private bool isMouseVisible;

    [Header("Menus, Animators y Botones")]
    public List<GameObject> Menus;
    private List<Animator> MenusAnimators = new List<Animator>();
    private List<List<GameObject>> ButtonsMenus = new List<List<GameObject>>();
    private Dictionary<string, int> menuIndexLookup;

    private void Awake()
    {
        GameObject inputManager = GameObject.Find("InputManager");
        if (inputManager != null)
        {
            playerInput = inputManager.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("PlayerInput component not found on InputManager.");
            }
        }
        else
        {
            Debug.LogError("InputManager GameObject not found.");
        }

        // Inicializar el diccionario para evitar NullReferenceException
        menuIndexLookup = new Dictionary<string, int>();

        MenusAnimators.Clear(); // Asegúrate de limpiar la lista antes de llenarla
        ButtonsMenus.Clear();   // Asegúrate de limpiar la lista antes de llenarla

        for (int i = 0; i < Menus.Count; i++)
        {
            GameObject menu = Menus[i];

            if (menu == null)
            {
                Debug.LogWarning($"Menu at index {i} is null.");
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
        Close_Charge();
        if(_inputType == InputType.Keyboard)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isMouseVisible = false;
        }
        else if(_inputType == InputType.Mouse)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isMouseVisible = true;
        }
    }

    private void Update()
    {
        if (InputManager.GetInstance().GetPausePressed() && _menuType == MenuType.PauseMenu)
        {
            if (GameManager.Instance._gameState == GameStates.Pause)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        
        if (GameManager.Instance._gameState == GameStates.Pause || _menuType == MenuType.MainMenu)
        {
            //mouseState = EventSystem.current.IsPointerOverGameObject();
            if (IsPointerOverScreen())
            {
                // Pointer is over the screen
                mouseState = false;
            }
            else
            {
                // Pointer is not over the screen
                mouseState = true;
            }

            if(_inputType == InputType.Automatic)
            {
                alternateButtons();
                alternateMouseVisibility();
            }
        }
    }

    private bool IsPointerOverScreen()
    {
        // Get the mouse position
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Check if the mouse position is within the screen bounds
        return mousePosition.x >= 0 && mousePosition.x <= Screen.width &&
            mousePosition.y >= 0 && mousePosition.y <= Screen.height;
    }

    void OnGUI()
    {
        if (!isMouseVisible)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void PauseGame()
    {
        GameManager.Instance._gameState = GameStates.Pause;
        Menus[0].SetActive(true);

        List<GameObject> Buttons = ButtonsMenus[0];
        GameObject firstButtonMenu = Buttons[0];

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButtonMenu);

        playerInput.SwitchCurrentActionMap("Pause");
        print(playerInput.currentActionMap);
    }

    public void ResumeGame()
    {
        GameManager.Instance._gameState = GameStates.Gameplay;

        Close_Charge();
        playerInput.SwitchCurrentActionMap("Gameplay");
        print(playerInput.currentActionMap);
    }

    public void MenuIgnition()
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
                Debug.LogWarning($"MenuAIndex {MenuAIndex} is out of range.");
            }
        }
        else
        {
            Debug.LogWarning($"MenuActivate with name {IgnitedMenu.name} not found in menuIndexLookup.");
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

    public void MenuTransition(GameObject MenuActivate)
    {
        // Obtener el botón que activó la transición
        GameObject currentButton = GetCurrentButton();
        if (currentButton == null)
        {
            Debug.LogWarning("No button selected.");
            return;
        }

        // Encontrar el MenuDeactivate correspondiente basado en el botón actual
        GameObject MenuDeactivate = FindMenuDeactivateFromButton(currentButton);

        if (MenuDeactivate == null)
        {
            Debug.LogWarning("MenuDeactivate could not be found.");
            return;
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
                Debug.LogWarning($"MenuAIndex {MenuAIndex} is out of range.");
            }
        }
        else
        {
            Debug.LogWarning($"MenuActivate with name {MenuActivate.name} not found in menuIndexLookup.");
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
                Debug.LogWarning($"MenuBIndex {MenuBIndex} is out of range.");
            }
        }
        else    
        {
            Debug.LogWarning($"MenuDeactivate with name {MenuDeactivate.name} not found in menuIndexLookup.");
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
    public void MenuTransitionOutside(GameObject MenuActivate, GameObject MenuDeactivat)
    {
        // Obtener el botón que activó la transición
        GameObject currentButton = GetCurrentButton();
        if (currentButton == null)
        {
            Debug.LogError("No button selected.");
            return;
        }

        // Encontrar el MenuDeactivate correspondiente basado en el botón actual
        GameObject MenuDeactivate = MenuDeactivat;

        if (MenuDeactivate == null)
        {
            Debug.LogError("MenuDeactivate could not be found.");
            return;
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
                Debug.LogError($"MenuAIndex {MenuAIndex} is out of range.");
            }
        }
        else
        {
            Debug.LogError($"MenuActivate with name {MenuActivate.name} not found in menuIndexLookup.");
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
                Debug.LogError($"MenuBIndex {MenuBIndex} is out of range.");
            }
        }
        else    
        {
            Debug.LogError($"MenuDeactivate with name {MenuDeactivate.name} not found in menuIndexLookup.");
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

    private GameObject GetCurrentButton()
    {
        // Obtener el botón actual seleccionado o el que se ha clicado
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            return EventSystem.current.currentSelectedGameObject;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
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
                    return result.gameObject;
                }
            }
        }

        return null;
    }

    private GameObject FindMenuDeactivateFromButton(GameObject button)
    {
        foreach (var menu in Menus)
        {
            int menuIndex = menuIndexLookup[menu.name];
            List<GameObject> buttons = ButtonsMenus[menuIndex];
            
            if (buttons.Contains(button))
            {
                Debug.Log($"Menu to deactivate found: {menu.name}");
                return menu;
            }
        }

        Debug.LogError("MenuDeactivate could not be found.");
        return null;
    }

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

        if(_inputType != InputType.Mouse)
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
        if(_inputType != InputType.Mouse)
        {
            GameObject firstButton = GetFirstButton(MenuActivate);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
            print("Botón seleccionado");
        }
        
        playerInput.actions.Enable();
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
    
    private IEnumerator DOTweenIgnition(GameObject MenuActivate)
    {
        playerInput.actions.Disable();

        // Ejemplo de animación simple con DOTween
        MenuActivate.SetActive(true);

        // Configura la animación de DOTween aquí
        // Puedes ajustar los valores según la animación deseada
        MenuActivate.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad);

        yield return new WaitForSeconds(0.5f);
        
        GameObject firstButton = GetFirstButton(MenuActivate);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButton);
        
        playerInput.actions.Enable();
    }

    private IEnumerator DOTweenTransition(GameObject MenuActivate, GameObject MenuDeactivate)
    {
        playerInput.actions.Disable();

        // Ejemplo de animación simple con DOTween
        MenuActivate.SetActive(true);

        // Configura la animación de DOTween aquí
        // Puedes ajustar los valores según la animación deseada
        MenuActivate.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad);
        MenuDeactivate.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);

        yield return new WaitForSeconds(0.5f);

        MenuDeactivate.SetActive(false);
        
        GameObject firstButton = GetFirstButton(MenuActivate);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButton);
        
        playerInput.actions.Enable();
    }

    private GameObject FindSimilarButton(GameObject MenuDeactivate)
    {
        foreach (var menuButtons in ButtonsMenus)
        {
            foreach (var button in menuButtons)
            {
                if (button.name.Contains(MenuDeactivate.name))
                {
                    return button;
                }
            }
        }
        return null;
    }

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
            GameManager.Instance._gameState = GameStates.Pause;
        }
    }

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

    public void GoMenu()
    {
    #if UNITY_WEBGL
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
    #elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenuPC");
    #endif
    }
    public void ChangeScene(string n)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(n);
    }
    public void NextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void RestartLevel()
    {
        int actualScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(actualScene);
    }
    public void ReloadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public GameObject AccessInnerList(int outerIndex, int innerIndex)
    {
        if (outerIndex < ButtonsMenus.Count)
        {
            List<GameObject> innerList = ButtonsMenus[outerIndex];
            if (innerIndex < innerList.Count)
            {
                GameObject button = innerList[innerIndex];
                Debug.Log("Accessed button: " + button.name);
                return button;
            }
            else
            {
                Debug.LogError("Inner index out of bounds");
            }
        }
        else
        {
            Debug.LogError("Outer index out of bounds");
        }

        return null;
    }

    public void alternateButtons()
    {
        if (mouseState)
        {
            EventSystem.current.SetSelectedGameObject(null);
            mouseByPass = true;
        }
        else if (!mouseState && mouseByPass)
        {
            foreach (var menu in Menus)
            {
                int i = Menus.IndexOf(menu);
                if (menu.activeSelf)
                {
                    GameObject button = AccessInnerList(i, 0);
                    if (button != null)
                    {
                        EventSystem.current.SetSelectedGameObject(button);
                    }
                }
            }
            mouseByPass = false;
        }
    }

    private void alternateMouseVisibility()
    {
        if (GameManager.Instance._gameState == GameStates.Pause)
        {
            if (Input.anyKeyDown && !Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                isMouseVisible = false;
            }

            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 || Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                isMouseVisible = true;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isMouseVisible = false;
        }
    }
}