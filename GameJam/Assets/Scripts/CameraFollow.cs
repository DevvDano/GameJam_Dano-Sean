using UnityEngine;

public class Follow_player : MonoBehaviour {

    public Transform player;

    // Update is called once per frame
    void LateUpdate () {
        // Follow the player with an offset, but keep y fixed
        transform.position = new Vector3(player.transform.position.x, 4.25f, -10);
    }
}