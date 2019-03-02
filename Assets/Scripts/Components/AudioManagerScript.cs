using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManagerScript : MonoBehaviour
{
    public AudioMixer mixer;
    AudioMixerGroup explore;
    AudioMixerGroup home;

    GameObject homeLoop;
    GameObject exploreLoop;

    GameObject player;

    float exploreVolume = -80;
    float homeVolume = 0;

    float percentExplore = 0;
    bool sliding = false;
    string slideDirection = "";

    float slideSpeed = 1;

    // Start is called before the first frame update
    void Start()
    {
      homeLoop = GameObject.Find("HomeLoop");
      exploreLoop = GameObject.Find("ExploreLoop");
      //explore = exploreLoop.GetComponent<AudioMixerGroup>()
      //home =
      setSliders();

    }

    // Update is called once per frame
    void Update()
    {
      //testSlide();
      if(sliding)
      {
        if (slideDirection == "home")
        {
          slideTowardsHome();
        }
        if (slideDirection == "explore")
        {
          slideTowardsExplore();
        }
      }
    }

    void testSlide()
    {
      if(Input.GetKeyDown("1") )
      {
        startSlide("home");
      }
      if(Input.GetKeyDown("2") )
      {
        startSlide("explore");
      }
    }

    public void setHomeStatus(bool atHome)
    {
      if (atHome)
      {
        startSlide("home");
      } else {
        startSlide("explore");
      }
    }

    void startSlide(string direction)
    {
      if (direction == "home")
      {

      }
      if (direction == "explore")
      {

      }
      slideDirection = direction;
      sliding = true;
    }

    void slideTowardsHome()
    {
      if (homeVolume <= 0)
      {
        homeVolume += 2 * slideSpeed;
        exploreVolume -= slideSpeed;
      } else {
        homeVolume = 0;
        exploreVolume -= slideSpeed;
        if (exploreVolume <= -80)
          exploreVolume = -80;
          slideDirection = "";
          sliding = false;
      }
      setSliders();
    }

    void slideTowardsExplore()
    {
      if (exploreVolume <= 0)
      {
        exploreVolume += 2 * slideSpeed;
        homeVolume -= slideSpeed;
      } else {
        exploreVolume = 0;
        homeVolume -= slideSpeed;
        if (homeVolume <= -80)
        {
          homeVolume = -80;
          slideDirection = "";
          sliding = false;
        }
      }
      setSliders();
    }

    void setSliders()
    {
      mixer.SetFloat("exploreVolume", exploreVolume);
      mixer.SetFloat("homeVolume", homeVolume);
    }
}
