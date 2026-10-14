using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource; // for background music
    public AudioSource sfxSource; // for sound effects

    [Header("Audio Clips")]
    public AudioClip backgroundMusic; // loops through the whole game
    public AudioClip treasureFoundSound; // when treasure is found 
    public AudioClip gameOverSound; // when game ends
    public AudioClip wrongAnswerSound; // when quiz is answered incorrectly
    public AudioClip winningMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AudioManager initialized.");

            // Validate audio components
            ValidateAudioSetup();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ValidateAudioSetup()
    {
        // Add AudioSource components if they are missing
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true; // BGM should loop by default
            Debug.LogWarning("AudioManager: BGM AudioSource was missing, added one automatically.");
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false; // SFX should not loop by default
            sfxSource.volume = 1.0f; // Ensure volume is at max
            Debug.LogWarning("AudioManager: SFX AudioSource was missing, added one automatically.");
        }

        // Log warnings if audio clips are not assigned
        if (backgroundMusic == null) Debug.LogWarning("AudioManager: Background music clip is missing!");
        if (treasureFoundSound == null) Debug.LogWarning("AudioManager: Treasure found sound clip is missing!");
        if (gameOverSound == null) Debug.LogWarning("AudioManager: Game over sound clip is missing!");
        if (wrongAnswerSound == null) Debug.LogWarning("AudioManager: Wrong answer sound clip is missing!");
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        Debug.Log("Attempting to play background music");
        if (bgmSource != null && backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.Play();
            Debug.Log("Background music started playing.");
        }
        else
        {
            Debug.LogError("Cannot play background music - missing components or clip.");
        }
    }

    // Restored this method as it might be called from Tapper
    public void PlayTreasureSound()
    {
        Debug.Log("Attempting to play treasure sound");
        if (sfxSource != null && treasureFoundSound != null)
        {
            sfxSource.PlayOneShot(treasureFoundSound, 1.0f);
            Debug.Log("Treasure sound played.");
        }
        else
        {
            Debug.LogError("Cannot play treasure sound - missing SFX source or clip.");
        }
    }

    public void PlayGameOverSound()
    {
        Debug.Log("Attempting to play game over sound");
        if (sfxSource != null && gameOverSound != null)
        {
            sfxSource.PlayOneShot(gameOverSound, 1.0f);
            Debug.Log("Game over sound played.");
        }
        else
        {
            Debug.LogError("Cannot play game over sound - missing SFX source or clip.");
        }
    }

    public void PlayCorrectAnswerSound()
    {
        Debug.Log("AudioManager: Attempting to play correct answer sound");

        // Ensure we have the necessary components
        if (sfxSource == null)
        {
            Debug.LogError("AudioManager: sfxSource is null!");
            return;
        }

        if (treasureFoundSound == null)
        {
            Debug.LogError("AudioManager: treasureFoundSound clip is null!");
            return;
        }

        // Force stop any current sfx
        sfxSource.Stop();

        // Play the sound directly instead of using PlayOneShot
        sfxSource.clip = treasureFoundSound;
        sfxSource.loop = false;
        sfxSource.volume = 1.0f; // Ensure full volume
        sfxSource.Play();

        Debug.Log("AudioManager: Correct answer sound played.");
    }

    public void PlayWrongAnswerSound()
    {
        Debug.Log("Attempting to play wrong answer sound");
        if (sfxSource != null && wrongAnswerSound != null)
        {
            // Force stop any current sfx
            sfxSource.Stop();

            sfxSource.PlayOneShot(wrongAnswerSound, 1.0f);
            Debug.Log("Wrong answer sound played.");
        }
        else if (sfxSource != null && gameOverSound != null)
        {
            // Fallback to game over sound if wrong answer sound is missing
            sfxSource.Stop();
            sfxSource.PlayOneShot(gameOverSound, 1.0f);
            Debug.Log("Fallback: Using game over sound for wrong answer.");
        }
        else
        {
            Debug.LogError("Cannot play wrong answer sound - missing SFX source or clip.");
        }
    }

    public void PlayWinningMusic()
    {
        Debug.Log("Attempting to play winning music");
        if (sfxSource != null && winningMusic != null)
        {
            // Stop background music first
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.Stop();
            }

            // Play winning music with looping
            sfxSource.loop = true;
            sfxSource.clip = winningMusic;
            sfxSource.Play();
            Debug.Log("Winning music started playing.");
        }
        else if (sfxSource != null && treasureFoundSound != null)
        {
            // Fallback to treasure sound if winning music is missing
            sfxSource.PlayOneShot(treasureFoundSound);
            Debug.Log("Fallback: Using treasure sound for winning music.");
        }
        else
        {
            Debug.LogError("Cannot play winning music - missing SFX source or clip.");
        }
    }

    // General purpose sound playing method
    public void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, 1.0f);
            Debug.Log($"Playing sound clip: {clip.name}");
        }
        else
        {
            Debug.LogError("Cannot play sound - missing SFX source or clip.");
        }
    }

    // Control volume methods
    public void SetMusicVolume(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = Mathf.Clamp01(volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }
}
