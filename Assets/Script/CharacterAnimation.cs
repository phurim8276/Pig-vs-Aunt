using UnityEngine;
using Spine.Unity;  // Needed for SkeletonAnimation

public class CharacterAnimation : MonoBehaviour
{
    [Header("Spine Animation")]
    public SkeletonAnimation skeletonAnimation;

    [Header("Animation Names")]
    public string idleAnimation = "idle";
    public string shootAnimation = "shoot";
    public string hitAnimation = "hit";
    public string winAnimation = "win";
    public string loseAnimation = "lose";
    public string mockeryAnimation = "mockery";

    private string currentAnimation;

    void Start()
    {
        PlayAnimation(idleAnimation, true);
    }

    /// <summary>
    /// Play the specified animation.
    /// </summary>
    public void PlayAnimation(string animationName, bool loop)
    {
        if (skeletonAnimation == null) return;

        // Avoid restarting the same animation
        if (currentAnimation == animationName) return;

        currentAnimation = animationName;
        skeletonAnimation.state.SetAnimation(0, animationName, loop);
    }

    // Convenient methods for each state
    public void PlayIdle() => PlayAnimation(idleAnimation, true);
    public void PlayShoot()
    {
        if (skeletonAnimation == null) return;
        currentAnimation = shootAnimation;
        skeletonAnimation.state.SetAnimation(0, shootAnimation, false)
            .Complete += (trackEntry) => PlayIdle();
    }

    public void PlayHit(System.Action onComplete = null)
    {
        if (skeletonAnimation == null) return;
        currentAnimation = hitAnimation;
        skeletonAnimation.state.SetAnimation(0, hitAnimation, false)
            .Complete += (entry) =>
            {
                PlayIdle();
                onComplete?.Invoke();
            };
    }

    public void PlayWin() => PlayAnimation(winAnimation, true);
    public void PlayLose() => PlayAnimation(loseAnimation, true);
    public void PlayMockery(System.Action onComplete = null)
    {
        if (skeletonAnimation == null) return;
        currentAnimation = mockeryAnimation;
        var trackEntry = skeletonAnimation.state.SetAnimation(0, mockeryAnimation, false);
        trackEntry.Complete += (entry) =>
        {
            PlayIdle();
            onComplete?.Invoke();
        };
    }

}
