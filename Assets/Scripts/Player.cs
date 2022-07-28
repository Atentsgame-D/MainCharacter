using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class Player : MonoBehaviour
{
    PlayerInputActions actions = null;
    Animator anim;
    GameObject useText;
    CharacterController controller;
    SkillCoolTimeManager coolTime;
    public GameManager manager;
    public GameObject scanObj;

    Vector3 inputDir = Vector3.zero;
    Quaternion targetRotation = Quaternion.identity;

    // 공격력 ---------------------
    float damage = 20.0f;
    public float Damage => damage;

    //float skillDamage = 0.0f;
    // 캐릭터 HP----------------------------
    float maxHP = 500.0f; 
    float hp = 100.0f;
    public float MaxHP { get => maxHP; }
    public float Hp 
    { 
        get => hp; 
        set
        {
            if (hp != value)
            {
                hp = Mathf.Clamp(value, 0.0f, maxHP);
                OnHpChange?.Invoke(hp);
            }
        }
    }

    public System.Action<float> OnHpChange;

    public void takeDamage(float damage)
    {
        Hp -= damage;
    }

    // 스킬용 변수------------------------------
    public bool gianHP = false;
    bool Onskill01 = false;
    public float skill01Distance = 10.0f;
    //캐릭터 MP------------------------------
    float maxMP = 200.0f;
    float mp = 50.0f;
    public float MaxMP { get => maxMP; }
    public float Mp 
    { 
        get => mp;
        set
        {
            if (mp != value)
            {
                mp = Mathf.Clamp(value, 0.0f, maxMP);
                OnMpChange?.Invoke(mp);
            }
        }
    }
    public System.Action<float> OnMpChange;
    //---------------------------------------
    enum MoveMode
    {
        Walk = 0,
        Run
    }

    MoveMode moveMode = MoveMode.Walk;
    //캐릭터 이동 관련---------------------------------------
    float speed = 5.0f;
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    public float turnSpeed = 15.0f;

    public float jumpPower = 5.0f;
    public float gravity = -9.81f;
    //---------------------------------------
    public bool tryUse = false;
    public bool isTrigger = false;
    //---------------------------------------


    private void Awake()
    {
        coolTime = GameObject.Find("SkillInfo").GetComponent<SkillCoolTimeManager>();
        actions = new PlayerInputActions();
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        useText = GameObject.Find("UseText_GameObject");
    }
    private void Start()
    {
        useText.gameObject.SetActive(false);
        manager.talkPanel.SetActive(false);
    }
    private void Update()
    {
        inputDir.y -= 9.8f * Time.deltaTime;
        if (inputDir.sqrMagnitude > 0.0f)
        {
            if (moveMode == MoveMode.Run)
            {
                speed = runSpeed;
                anim.SetBool("OnRun",true);
            }
            else if (moveMode == MoveMode.Walk)
            {   
                speed = walkSpeed;
                anim.SetBool("OnRun", false);
            }
            controller.Move(speed * Time.deltaTime * inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        //캐릭터가 땅에 붙어있지 않으면 중력 작용
        if (!controller.isGrounded) 
        {
            inputDir.y += gravity * Time.deltaTime;
        }

        if (Onskill01)
        {
            transform.Translate(skill01Distance * Vector3.forward * Time.deltaTime, Space.Self);
        }

        ScanObject();
        if (scanObj == null)
        {
            manager.talkPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        actions.PlayerMove.Enable();
        actions.PlayerMove.Move.performed += OnMoveInput;                //wasd
        actions.PlayerMove.Move.canceled += OnMoveInput;

        actions.Player.Enable();
        actions.Player.Use.performed += OnUseInput;                  //f키
        actions.Player.Jump.performed += OnJump;
        actions.Player.MoveModeChange.performed += OnMoveModeChange; // 왼쪽 쉬프트 
        actions.Player.Skill1.performed += OnSkill_1;
        actions.Player.Skill2.performed += OnSkill_2;
        actions.Player.Skill3.performed += OnSkill_3;
        actions.Player.Skill4.performed += OnSkill_4;
        actions.Player.NormalAttack.performed += OnNormalAttack;
    }

    private void OnDisable()
    {
        actions.Player.NormalAttack.performed -= OnNormalAttack;
        actions.Player.Skill4.performed -= OnSkill_4;
        actions.Player.Skill3.performed -= OnSkill_3;
        actions.Player.Skill2.performed -= OnSkill_2;
        actions.Player.Skill1.performed -= OnSkill_1;
        actions.Player.MoveModeChange.performed -= OnMoveModeChange;
        actions.Player.Jump.performed -= OnJump;
        actions.Player.Use.performed -= OnUseInput;
        actions.Player.Disable();

        actions.PlayerMove.Move.canceled -= OnMoveInput;
        actions.PlayerMove.Move.performed -= OnMoveInput;
        actions.PlayerMove.Disable();
    }

    private void OnMoveInput(InputAction.CallbackContext context) // wasd를 눌러 이동
    {
        Vector2 input = context.ReadValue<Vector2>();

        inputDir.x = input.x;
        inputDir.y = 0.0f;
        inputDir.z = input.y;

        if (inputDir.sqrMagnitude > 0.0f)
        {
            inputDir = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0) * inputDir;
            targetRotation = Quaternion.LookRotation(inputDir);
            anim.SetBool("OnMove", true);
        }
        else
        {
            anim.SetBool("OnMove", false);
            moveMode = MoveMode.Walk;
        }
    }

    private void OnUseInput(InputAction.CallbackContext context)
    {
        Use();
    }

    public void Use()
    {
        if (isTrigger)
        {
            if (tryUse)
            {
                tryUse = false;
            }
            else
            {
                tryUse = true;
            }
            useText.gameObject.SetActive(!tryUse);
        }
        if(scanObj != null)
        {
            manager.Action(scanObj);
        }
    }
    //------------------
    private void OnJump(InputAction.CallbackContext _)
    {
        if (controller.isGrounded)
        {
            inputDir.y = jumpPower; // y축 이동방향을 점프높이로 설정하여 올라가다가 다시 내려오게 만듦 = 점프
            anim.SetTrigger("OnJump");
        }
    }
    private void OnMoveModeChange(InputAction.CallbackContext context) // 쉬프트 키로 달리기 토글 설정. 기본 걷기상태
    {
        if (moveMode == MoveMode.Walk)
        {
            moveMode = MoveMode.Run;
        }
        else
        {
            moveMode = MoveMode.Walk;
        }
    }

    // 스킬 세팅
    private void OnSkill_1(InputAction.CallbackContext _) // 돌진
    {
        if (coolTime.skill1_CoolTime <= 0.0f && Mp > 30.0f && controller.isGrounded)
        {
            Mp -= 30.0f;
            StartCoroutine(Skill01());
            coolTime.skill1();
        }
    }
    IEnumerator Skill01()
    {
        actions.PlayerMove.Disable();
        actions.Player.Disable();
        anim.SetBool("OnSkill1",true);
        Onskill01 = true;
        yield return new WaitForSeconds(1.0f);
        Onskill01 = false;
        anim.SetBool("OnSkill1", false);
        actions.Player.Enable();
        actions.PlayerMove.Enable();

    }

    private void OnSkill_2(InputAction.CallbackContext _) // 강공격?
    {
        if (coolTime.skill2_CoolTime <= 0.0f && Mp > 30.0f)
        {
            Mp -= 30.0f;
            StartCoroutine(Skill02());
            coolTime.skill2();
        }
    }
    IEnumerator Skill02()
    {
        damage *= 0.5f;
        anim.SetBool("OnSkill2", true);
        actions.Player.Disable();
        yield return new WaitForSeconds(3.0f);
        actions.Player.Enable();
        anim.SetBool("OnSkill2", false);
        damage *= 2.0f;
    }

    private void OnSkill_3(InputAction.CallbackContext _) // 흡혈 버프
    {
        if (coolTime.skill3_CoolTime <= 0.0f && Mp > 50.0f)
        {
            Mp -= 50.0f;
            StartCoroutine(Skill03());
            coolTime.skill3();            
        }
    }

    IEnumerator Skill03()
    {
        gianHP = true;
        yield return new WaitForSeconds(6.0f);
        gianHP = false;
    }

    private void OnSkill_4(InputAction.CallbackContext _) // 도약
    {
        if (coolTime.skill4_CoolTime <= 0.0f && controller.isGrounded && Mp > 30.0f)
        {
            Mp -= 30.0f;
            inputDir.y = jumpPower * 1.25f; //기존 점프의 1.25배 높이 뜀
            coolTime.skill4();
        }
    }

    private void OnNormalAttack(InputAction.CallbackContext _)
    {
        anim.SetTrigger("OnNormalAttack");
    }


    //------------------
    private void OnTriggerEnter(Collider other)
    {
        isTrigger = true;
        if (other.CompareTag("Store"))
        {
            useText.SetActive(true);
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        isTrigger = false;
        tryUse = false;
        if (other.CompareTag("Store"))
        {
            useText.SetActive(false);
        }
            
    }

    void ScanObject()
    {
        Ray ray = new(transform.position, transform.forward);
        ray.origin += Vector3.up * 0.5f;
        if (Physics.Raycast(ray, out RaycastHit hit, 1.0f, LayerMask.GetMask("Object")))
        {
            if (hit.collider != null)
            {
                scanObj = hit.collider.gameObject;
            }
            else
                scanObj = null;
        }
        else
        {
            scanObj = null;
        }
    }

}
