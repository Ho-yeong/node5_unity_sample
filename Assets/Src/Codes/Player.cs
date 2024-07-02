using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public string deviceId;
    public RuntimeAnimatorController[] animCon;

    private Vector3 lastPosition;
    private Vector3 currentPosition;


    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    TextMeshPro myText;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        myText = GetComponentInChildren<TextMeshPro>();
    }

    void OnEnable() {

        if (deviceId.Length > 5) {
            myText.text = deviceId[..5];
        } else {
            myText.text = deviceId;
        }
        myText.GetComponent<MeshRenderer>().sortingOrder = 6;
        
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.isLive) {
            return;
        }
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

        // 위치 이동 패킷 전송 -> 서버로
        NetworkManager.instance.SendLocationUpdatePacket(rigid.position.x, rigid.position.y);
    }

    void FixedUpdate() {
        if (!GameManager.instance.isLive) {
            return;
        }
        // 힘을 준다.
        // rigid.AddForce(inputVec);

        // 속도 제어
        // rigid.velocity = inputVec;

        // 위치 이동
        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    // Update가 끝난이후 적용
    void LateUpdate() {
        if (!GameManager.instance.isLive) {
            return;
        }

        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0) {
            spriter.flipX = inputVec.x < 0;
        }
    }

    // 서버로부터 위치 업데이트를 수신할 때 호출될 메서드
    public void UpdatePosition(float x, float y)
    {
        lastPosition = currentPosition;
        currentPosition = new Vector3(x, y);
        transform.position = currentPosition;

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (!GameManager.instance.isLive) {
            return;
        }

        Vector2 inputVec = currentPosition - lastPosition;

        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }


    void OnCollisionStay2D(Collision2D collision) {
        if (!GameManager.instance.isLive) {
            return;
        }
    }
}
