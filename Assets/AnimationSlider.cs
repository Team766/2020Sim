using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

[RequireComponent(typeof(Slider))]
public class AnimationSlider : NetworkBehaviour
{
    public Animator anim;

    public Vector2 guiPosition;

    [SyncVar]
    public float animationTime;

    void Start () {
        anim.speed = 0;
        GetComponent<Slider>().onValueChanged.AddListener(delegate {AnimateOnSliderValue(); });
    }

    void Update() {
        // NOTE(ryan.cahoon, 2020-09-23): This seems to put the GUI in the right
        // place on WebGL. This shouldn't be necessary (and it works fine
        // without this on desktop), but it's cheap to do and there are more
        // important things to work on.
        GetComponent<RectTransform>().anchoredPosition = guiPosition;

        anim.Play("", -1, Mathf.Min(0.999f, animationTime));
    }

    void AnimateOnSliderValue () {
        SetAnimationTime(GetComponent<Slider>().value);
    }

    [Command(requiresAuthority = false)]
    void SetAnimationTime(float time) {
        animationTime = time;
    }
 }
