using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Google;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LoginWithGoogle : MonoBehaviour
{
    public string GoogleAPI = "add your webclient id here";
    private GoogleSignInConfiguration configuration;

    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;

    public TextMeshProUGUI Username;

    public Image UserProfilePic;
    private string imageUrl;
    private bool isGoogleSignInInitialized = false;

    [SerializeField] GameObject LoginPanel, ProfilePanel;

    public bool isGuestLogin = false;

    private UIManager uIManager;
    private void Start()
    {
        InitFirebase();
        uIManager = FindObjectOfType<UIManager>();
    }

    void InitFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    }
    public void GuestLogin()
    {
        LoginPanel.SetActive(false);
        ProfilePanel.SetActive(true);
        isGuestLogin = true;
        if (uIManager != null)
        {
            uIManager.TriggerLogButton();
        }
    }
    public void Login()
    {
        if (!isGoogleSignInInitialized)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = GoogleAPI,
                RequestEmail = true
            };

            isGoogleSignInInitialized = true;
        }
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = GoogleAPI
        };
        GoogleSignIn.Configuration.RequestEmail = true;

        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

        TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
        signIn.ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                signInCompleted.SetCanceled();
                Debug.Log("Cancelled");
            }
            else if (task.IsFaulted)
            {
                signInCompleted.SetException(task.Exception);

                Debug.Log("Faulted " + task.Exception);
            }
            else
            {
                Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>)task).Result.IdToken, null);
                auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
                {
                    if (authTask.IsCanceled)
                    {
                        signInCompleted.SetCanceled();
                    }
                    else if (authTask.IsFaulted)
                    {
                        signInCompleted.SetException(authTask.Exception);
                        Debug.Log("Faulted In Auth " + task.Exception);
                    }
                    else
                    {
                        signInCompleted.SetResult(((Task<FirebaseUser>)authTask).Result);
                        Debug.Log("Success");
                        user = auth.CurrentUser;
                        Username.text = user.DisplayName;
                        //UserEmail.text = user.Email;
                        isGuestLogin = false;
                        
                        StartCoroutine(LoadImage(CheckImageUrl(user.PhotoUrl.ToString())));
                    }
                });
            }
        });
    }
    private string CheckImageUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            return url;
        }
        return imageUrl;
    }

    IEnumerator LoadImage(string imageUri)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUri);
        
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            // Use the loaded texture here
            Debug.Log("Image loaded successfully");
            UserProfilePic.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));

            if (LoginPanel.activeSelf)
            {
                LoginPanel.SetActive(false);
            }
            if (!ProfilePanel.activeSelf)
            {
                ProfilePanel.SetActive(true);
            }
            if (uIManager != null)
            {
                uIManager.TriggerLogButton();
            }
        }
        else
        {
            Debug.Log("Error loading image: " + www.error);
        }

        
    }
    public void Logout()
    {
        // Reset UI elements
        Username.text = "Please Login";
        //UserEmail.text = "";
        UserProfilePic.sprite = null;

        Debug.Log("Signing out from Firebase and Google");

        // Sign out from Firebase Authentication
        auth.SignOut();

        // Sign out from Google Sign-In
        GoogleSignIn.DefaultInstance.SignOut();

        // Show login panel, hide profile panel
        LoginPanel.SetActive(true);
        ProfilePanel.SetActive(false);
    }
}