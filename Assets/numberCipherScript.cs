using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using numberCipher;

public class numberCipherScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;

    //Cubes
    public Animator[] cubeAnimation;
    private int direction = 0;
    private int cubeLevel = 0;
    private int timeDelay = 0;
    public TextMesh[] cubeNumbersDisplay;
    private int[] cubeNumbers = new int[12];
    private int[] currentlyDisplayed = new int[3];
    private int[] currentlyDisplayedIndex = new int[3];

    //Colored Lights
    public Renderer[] coloredLights;
    public Material[] lightOptions;
    public bool[] lightColorBool;
    private int lightLevel = 0;
    private int[] lightColorIndex = new int[3];
    public string[] lightColorNames;
    private int lightColorLevel = 0;
    private bool blueIsOn = false;
    private bool greenIsOn = false;
    private bool redIsOn = false;

    //Timer
    public Renderer[] timerBars;
    public Material[] timerColors;

    //Display & Buttons
    public KMSelectable executeButton;
    public KMSelectable cycleLeftButton;
    public KMSelectable cycleRightButton;
    public TextMesh displayedNumberText;
    private int displayedNumber = 0;
    private string correctAnswer = "";
    private bool executeLock = false;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved = false;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        executeButton.OnInteract += delegate () { OnExecuteButton(); return false; };
        cycleLeftButton.OnInteract += delegate () { OnCycleLeftButton(); return false; };
        cycleRightButton.OnInteract += delegate () { OnCycleRightButton(); return false; };
    }

    void Start()
    {
        foreach(Renderer bar in timerBars)
        {
            bar.material = timerColors[0];
        }
        StartCoroutine(CubeRotation());
        SelectCubeNumbers();
        SelectColors();
        Logic();
        displayedNumber = UnityEngine.Random.Range(0,10);
        displayedNumberText.text = displayedNumber.ToString();
    }

    public void OnExecuteButton()
    {
        GetComponent<KMSelectable>().AddInteractionPunch();
        if(moduleSolved || executeLock)
        {
            return;
        }
        if(correctAnswer == displayedNumberText.text)
        {
            Audio.PlaySoundAtTransform("steam", transform);
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[The Number Cipher #{0}] You submitted {1}. That is correct. Module solved.",moduleId, displayedNumberText.text);
            foreach(Renderer bar in timerBars)
            {
                bar.material = lightOptions[0];
            }
            foreach(Renderer lightColor in coloredLights)
            {
                lightColor.material = lightOptions[0];
            }
            moduleSolved = true;
            executeLock = true;
        }
        else
        {
            executeLock = true;
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[The Number Cipher #{0}] Strike! You submitted {1}. That is not correct. Module locked.",moduleId, displayedNumberText.text);
        }
    }

    public void OnCycleLeftButton()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch(0.5f);
        displayedNumber = (displayedNumber + 9) % 10;
        displayedNumberText.text = displayedNumber.ToString();
    }

    public void OnCycleRightButton()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch(0.5f);
        displayedNumber = (displayedNumber + 1) % 10;
        displayedNumberText.text = displayedNumber.ToString();
    }

    IEnumerator TimerColorsReset()
    {
        Audio.PlaySoundAtTransform("restart", transform);
        for(int i = 0; i < 5; i++)
        {
            timerBars[i].material = timerColors[0];
            timerBars[i+5].material = timerColors[0];
            yield return new WaitForSeconds(0.1f);
        }
    }

    void SelectCubeNumbers()
    {
        int increaser = 0;
        foreach(TextMesh cubeText in cubeNumbersDisplay)
        {
            int index = UnityEngine.Random.Range(1,10);
            cubeNumbers[increaser] = index;
            cubeText.text = index.ToString();
            increaser++;
        }

        currentlyDisplayedIndex[0] = 0;
        currentlyDisplayedIndex[1] = 4;
        currentlyDisplayedIndex[2] = 8;
        for(int i = 0; i < 3; i++)
        {
            currentlyDisplayed[i] = cubeNumbers[currentlyDisplayedIndex[i]];
        }
        Debug.LogFormat("[The Number Cipher #{0}] Displayed number: {1}{2}{3}.",moduleId, currentlyDisplayed[0], currentlyDisplayed[1], currentlyDisplayed[2]);
    }

    void SelectColors()
    {
        lightLevel = 0;
        lightColorLevel = 0;
        foreach(Renderer coloredLight in coloredLights)
        {
            int lightIndex = UnityEngine.Random.Range(0,4);
            coloredLight.material = lightOptions[lightIndex];
            lightColorBool[lightIndex + lightLevel] = true;
            lightLevel += 4;
            lightColorIndex[lightColorLevel] = lightIndex;
            lightColorLevel++;
        }
        Debug.LogFormat("[The Number Cipher #{0}] Displayed colors: {1}, {2}, {3}.",moduleId, lightColorNames[lightColorIndex[0]], lightColorNames[lightColorIndex[1]], lightColorNames[lightColorIndex[2]]);
    }

    IEnumerator CubeRotation()
    {
        while(moduleSolved == false)
        {
            cubeLevel = 0;
            timeDelay = UnityEngine.Random.Range(20,41);
            Debug.LogFormat("[The Number Cipher #{0}] Time Delay: {1}.", moduleId, timeDelay);
            yield return new WaitForSeconds(timeDelay - 10);
            for(int i = 4; i >= 0; i--)
            {
                if(moduleSolved == false)
                {
                    timerBars[i].material = timerColors[1];
                    timerBars[i + 5].material = timerColors[1];
                    Audio.PlaySoundAtTransform("beep", transform);
                }
                yield return new WaitForSeconds(2f);
            }
            executeLock = true;

            if(moduleSolved == false)
            {
                foreach(Animator cube in cubeAnimation)
                {
                    direction = UnityEngine.Random.Range(0,3);
                    if(direction == 0)
                    {
                        Audio.PlaySoundAtTransform("gear", transform);
                        cube.SetBool("forward", true);
                        yield return new WaitForSeconds(0.5f);
                        cube.SetBool("forward", false);
                        if(cubeLevel == 0)
                        {
                            currentlyDisplayedIndex[0] = (currentlyDisplayedIndex[0] + 1) % 4;
                        }
                        else if(cubeLevel == 1)
                        {
                            currentlyDisplayedIndex[1] = ((currentlyDisplayedIndex[1] + 1) % 4) + 4;
                        }
                        else
                        {
                            currentlyDisplayedIndex[2] = ((currentlyDisplayedIndex[2] + 1) % 4) + 8;
                        }
                        cubeLevel++;
                    }
                    else if(direction == 1)
                    {
                        Audio.PlaySoundAtTransform("gear", transform);
                        cube.SetBool("backward", true);
                        yield return new WaitForSeconds(0.5f);
                        cube.SetBool("backward", false);
                        if(cubeLevel == 0)
                        {
                            currentlyDisplayedIndex[0] = (currentlyDisplayedIndex[0] + 3) % 4;
                        }
                        else if(cubeLevel == 1)
                        {
                            currentlyDisplayedIndex[1] = ((currentlyDisplayedIndex[1] + 3) % 4) + 4;
                        }
                        else
                        {
                            currentlyDisplayedIndex[2] = ((currentlyDisplayedIndex[2] + 3) % 4) + 8;
                        }
                        cubeLevel++;
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.1f);
                        cubeLevel++;
                    }
                }
                for(int i = 0; i < 12; i++)
                {
                    lightColorBool[i] = false;
                }
                SelectColors();
                for(int i = 0; i < 3; i++)
                {
                    currentlyDisplayed[i] = cubeNumbers[currentlyDisplayedIndex[i]];
                }
                StartCoroutine(TimerColorsReset());
                Logic();
                Debug.LogFormat("[The Number Cipher #{0}] Displayed number: {1}{2}{3}.",moduleId, currentlyDisplayed[0], currentlyDisplayed[1], currentlyDisplayed[2]);
                executeLock = false;
            }
        }
    }

    void Logic()
    {
        blueIsOn = false;
        greenIsOn = false;
        redIsOn = false;
        if(lightColorBool[1] || lightColorBool[5] || lightColorBool[9])
        {
            blueIsOn = true;
        }
        if(lightColorBool[2] || lightColorBool[6] || lightColorBool[10])
        {
            greenIsOn = true;
        }
        if(lightColorBool[3] || lightColorBool[7] || lightColorBool[11])
        {
            redIsOn = true;
        }

        if(blueIsOn && greenIsOn && redIsOn)
        {
            correctAnswer = (((currentlyDisplayed[0] * currentlyDisplayed[1] * currentlyDisplayed[2]) - 1) % 9 + 1).ToString();
        }
        else if(blueIsOn && greenIsOn)
        {
            correctAnswer = (((currentlyDisplayed[0] * 10 + currentlyDisplayed[1]) * currentlyDisplayed[2]) % 10).ToString();
        }
        else if(blueIsOn && redIsOn)
        {
            correctAnswer = (((currentlyDisplayed[1] * 10 + currentlyDisplayed[2]) * currentlyDisplayed[0]) % 10).ToString();
        }
        else if(redIsOn && greenIsOn)
        {
            correctAnswer = (((currentlyDisplayed[0] + currentlyDisplayed[1] * currentlyDisplayed[2]) - 1) % 9 + 1).ToString();
        }
        else if(blueIsOn)
        {
            correctAnswer = ((currentlyDisplayed[0] + (currentlyDisplayed[1] * 10 + currentlyDisplayed[2])) % 10).ToString();
        }
        else if(redIsOn)
        {
            correctAnswer = (((currentlyDisplayed[0] * currentlyDisplayed[1] + currentlyDisplayed[2]) - 1) % 9 + 1).ToString();
        }
        else if(greenIsOn)
        {
            correctAnswer = ((((currentlyDisplayed[0] * 10) + currentlyDisplayed[1]) - currentlyDisplayed[2]) % 10).ToString();
        }
        else
        {
            correctAnswer = ((((currentlyDisplayed[0] * 100) + (currentlyDisplayed[1] * 10) + currentlyDisplayed[2]) - 1) % 9 + 1).ToString();
        }
        Debug.LogFormat("[The Number Cipher #{0}] Correct answer: {1}.",moduleId, correctAnswer);
    }

    private string TwitchHelpMessage = @"Use '!{0} submit 1' to submit a number.";

    IEnumerator ProcessTwitchCommand(string command)
    {
        var parts = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        int clicks = 0;
        if (parts.Length == 2 && parts[0] == "submit" && parts[1].Length == 1 && "0123456789".Contains(parts[1]))
        {
            while (parts[1] != displayedNumber.ToString())
            {
                if (clicks > 9) break;

                yield return null;
                OnCycleRightButton();
                clicks++;
                yield return new WaitForSeconds(.1f);
            }

            OnExecuteButton();
        }
    }
}
