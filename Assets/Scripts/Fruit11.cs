using UnityEngine;

public class Fruit11 : MonoBehaviour
{
    public GameObject whole;
    public GameObject sliced;
    public int fruitValue; // The value assigned to this fruit

    private Rigidbody fruitRigidbody;
    private Collider fruitCollider;
    private ParticleSystem fruitParticleEffect;

    private void Awake()
    {
        fruitRigidbody = GetComponent<Rigidbody>();
        fruitCollider = GetComponent<Collider>();
        fruitParticleEffect = GetComponentInChildren<ParticleSystem>();
    }

    private void Slice(Vector3 direction, Vector3 position, float force)
    {
        whole.SetActive(false);
        sliced.SetActive(true);

        fruitCollider.enabled = false;
        //fruitParticleEffect.Play();

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        sliced.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Rigidbody[] slices = sliced.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody slice in slices)
        {
            slice.velocity = fruitRigidbody.velocity;
            slice.AddForceAtPosition(direction * force, position, ForceMode.Impulse);
        }

        // Check if the sliced fruit's value matches the correct answer
        if (fruitValue == Level11.Instance.GetCurrentAnswer())
        {
            Debug.Log("Correct fruit sliced. Updating score and displaying the next question...");
            Level11.Instance.AddScore(1);
            Level11.Instance.DisplayNextQuestion();
        }
        else
        {
            Debug.Log($"Wrong fruit sliced. Fruit value: {fruitValue}, Expected: {Level11.Instance.GetCurrentAnswer()}");
            Level11.Instance.LoseHeart();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Blade blade = other.GetComponent<Blade>();
            Slice(blade.direction, blade.transform.position, blade.sliceForce);
        }
    }
}
