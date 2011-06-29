// Make a property that holds the audio clip
var walking : AudioClip;

function Update () {
}

function PlayWalkSound()
{
    audio.clip = walking;
    audio.Play();
}