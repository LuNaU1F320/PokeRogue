// using System.Collections;
// using System;
// using UnityEngine;
// using System.Linq;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;

// public enum BattleState_
// {
//     Start,
//     ActionSelection,
//     SkillSelection,
//     RunningTurn,
//     Busy,
//     PartyScreen,
//     SkillToForget,
//     ConfirmBox,
//     Evolution,
//     BattleOver,
//     ConfigSelection
// }
// public enum BattleAction_
// {
//     Skill,
//     SwitchPokemon,
//     UseItem,
//     Run
// }
// public class BattleSystem_Old : MonoBehaviour
// {
//     public static BattleSystem Inst;
//     [SerializeField] BattleUnit playerUnit;
//     [SerializeField] BattleUnit enemyUnit;
//     [SerializeField] BattleDialogBox dialogBox;
//     [SerializeField] ConfigPanel configPanel;
//     [SerializeField] GameObject Pokeball;
//     [SerializeField] GameObject ConfirmBox;

//     [SerializeField] SpriteRenderer PlayerSprite;
//     [SerializeField] SpriteRenderer TrainerSprite;

//     //Party
//     [SerializeField] PartyScreen partyScreen;
//     [SerializeField] SkillSelectScreen skillSelectScreen;

//     [HideInInspector] public BattleState state;
//     BattleState? preState;
//     int currentAction = 0;
//     int currentSkill = 0;
//     int currentMember = 0;
//     int currentSelection = 0;
//     int currentConfirm = 0;
//     int currentConfig = 0;
//     int skillCount = 0;
//     int escapeAttempts = 0;

//     PokemonParty playerParty;
//     PokemonParty trainerParty;
//     Pokemon wildPokemon;
//     bool isTrainerBattle = false;

//     PlayerCtrl player;
//     [SerializeField] Image PlayerImage;
//     TrainerCtrl trainer;

//     SkillBase skillToLearn;

//     private void Awake()
//     {
//         // Inst = this;
//         player = FindObjectOfType<PlayerCtrl>();
//     }
//     private void Start()
//     {
//         // player = FindObjectOfType<PlayerCtrl>();
//         state = BattleState.Start;
//         currentAction = 0;
//         PlayerImage.sprite = player.TrainerSprite;
//         PlayerImage.gameObject.SetActive(false);
//     }
//     public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
//     {
//         state = BattleState.Start;
//         isTrainerBattle = false;
//         this.playerParty = playerParty;
//         this.wildPokemon = wildPokemon;
//         isTrainerBattle = false;
//         StartCoroutine(SetUpBattle());
//     }
//     public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
//     {
//         this.playerParty = playerParty;
//         this.trainerParty = trainerParty;

//         isTrainerBattle = true;

//         player = playerParty.GetComponent<PlayerCtrl>();
//         trainer = trainerParty.GetComponent<TrainerCtrl>();

//         StartCoroutine(SetUpBattle());
//     }
//     public void Update()
//     {
//         if (state == BattleState.BattleOver || state == BattleState.Evolution)
//         {
//             return;
//         }
//         if (state == BattleState.ActionSelection)
//         {
//             HandleActionSelection();
//         }
//         else if (state == BattleState.SkillSelection)
//         {
//             HandleSkillSelection();
//         }
//         else if (state == BattleState.PartyScreen)
//         {
//             HandlePartyScreenSelection();
//         }
//         else if (state == BattleState.SkillToForget)
//         {
//             HandleLearnSkillSelection();
//         }
//         else if (state == BattleState.ConfirmBox)
//         {
//             HandleConfirmBoxSelection();
//         }
//         else if (state == BattleState.ConfigSelection)
//         {
//             HandleConfigSelection();
//         }

//         if (Input.GetKeyDown(KeyCode.Escape))
//         {
//             // Debug.Log(state);
//             if (state == BattleState.ConfigSelection)
//             {
//                 if (configPanel.state == ConfigState.Config_Right)
//                 {
//                     if (Input.GetKeyDown(KeyCode.Escape))
//                     {
//                         configPanel.Panel.SetActive(false);
//                         state = preState ?? BattleState.RunningTurn;
//                     }
//                 }
//             }
//             else
//             {
//                 configPanel.Panel.SetActive(true);
//                 preState = state;
//                 state = BattleState.ConfigSelection;
//                 currentConfig = 0;
//                 configPanel.state = ConfigState.Config_Right;
//             }
//         }
//         if (Input.GetKeyDown(KeyCode.G))
//         {
//             Debug.Log(currentMember);
//         }
//         if (Input.GetKeyDown(KeyCode.F))
//         {
//             Debug.Log(playerUnit.BattlePokemon.Attack);
//             Debug.Log(playerUnit.BattlePokemon.Rankup[0]);
//         }
//         if (Input.GetKeyDown(KeyCode.T))
//         {
//             Debug.Log($"{playerUnit.BattlePokemon.PokemonGen}");
//         }
//     }

//     public IEnumerator SetUpBattle()
//     {
//         if (isTrainerBattle == false)
//         {
//             playerUnit.SetUp(playerParty.GetHealthyPokemon());
//             enemyUnit.SetUp(wildPokemon);

//             dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);

//             skillCount = playerUnit.BattlePokemon.Skills.Count;

//             yield return dialogBox.TypeDialog($"앗! 야생 {enemyUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(enemyUnit.BattlePokemon.P_Base.PokemonName, "subject")} \n튀어나왔다!");
//         }
//         else
//         {
//             playerUnit.gameObject.SetActive(false);
//             enemyUnit.gameObject.SetActive(false);

//             PlayerSprite.gameObject.SetActive(true);
//             TrainerSprite.gameObject.SetActive(true);

//             PlayerSprite.sprite = player.TrainerSprite;
//             TrainerSprite.sprite = trainer.TrainerSprite;

//             yield return dialogBox.TypeDialog($"{trainer.TrainerName}{/*은는이가*/""}이 배틀을 걸어왔다!");
//         }
//         escapeAttempts = 0;
//         partyScreen.Init();
//         ActionSelection();
//         yield return null;
//     }
//     void BattleOver(bool won)
//     {
//         state = BattleState.BattleOver;
//         // StartCoroutine(playerParty.CheckForEvolutions());
//         GameManager.Inst.EndBattle(won);
//     }
//     void ActionSelection()
//     {
//         state = BattleState.ActionSelection;
//         dialogBox.EnableActionSelector(true);
//         StartCoroutine(dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 무엇을 할까?"));
//     }
//     void OpenPartyScreen()
//     {
//         state = BattleState.PartyScreen;
//         partyScreen.Init();
//         partyScreen.SetPartyData(playerParty.Party);
//         partyScreen.gameObject.SetActive(true);
//     }
//     void SkillSelection()
//     {
//         state = BattleState.SkillSelection;
//         dialogBox.EnableActionSelector(false);
//         dialogBox.EnableDialogText(false);
//         dialogBox.EnableSkillSelector(true);
//     }
//     public void ConfirmBoxSelection()
//     {
//         state = BattleState.ConfirmBox;
//         currentConfirm = 0;
//         ConfirmBox.SetActive(true);
//     }
//     IEnumerator ChooseSkillToForget(Pokemon pokemon, SkillBase newSkill)
//     {
//         state = BattleState.Busy;
//         yield return dialogBox.TypeDialog($"어느 기술을 잊게 하고싶은가?");
//         skillSelectScreen.gameObject.SetActive(true);
//         skillSelectScreen.SetPokemonData(pokemon);
//         skillSelectScreen.SetSkill(pokemon.Skills.Select(x => x.SkillBase).ToList(), newSkill);
//         skillToLearn = newSkill;

//         state = BattleState.SkillToForget;
//     }

//     #region BattleSystem
//     // IEnumerator RunTurns(BattleAction playerAction)
//     // {
//     //     state = BattleState.RunningTurn;
//     //     if (playerAction == BattleAction.Skill)
//     //     {

//     //         playerUnit.BattlePokemon.CurrentSkill = playerUnit.BattlePokemon.Skills[currentSkill];
//     //         enemyUnit.BattlePokemon.CurrentSkill = enemyUnit.BattlePokemon.GetRandomSkill();

//     //         int playerPriority = playerUnit.BattlePokemon.CurrentSkill.SkillBase.Priority;
//     //         int enemyPriority = enemyUnit.BattlePokemon.CurrentSkill.SkillBase.Priority;

//     //         if (playerUnit.BattlePokemon.CurrentSkill == null)
//     //         {
//     //             Debug.LogError("❌ CurrentSkill이 설정되지 않았습니다! 스킬 선택 없이 실행되려 하고 있어요.");
//     //             yield break; // 그냥 턴 무시하고 끝냄
//     //         }

//     //         // 스피드 체크
//     //         bool playerTurnFirst = true;
//     //         //우선도 체크
//     //         if (enemyPriority > playerPriority)
//     //         {
//     //             playerTurnFirst = false;
//     //         }
//     //         else if (playerPriority == enemyPriority)
//     //         {
//     //             if (playerUnit.BattlePokemon.Speed == enemyUnit.BattlePokemon.Speed)
//     //             {
//     //                 playerTurnFirst = UnityEngine.Random.Range(0, 2) == 0;
//     //             }
//     //             playerTurnFirst = playerUnit.BattlePokemon.Speed > enemyUnit.BattlePokemon.Speed;
//     //         }

//     //         var firstUnit = playerTurnFirst ? playerUnit : enemyUnit;
//     //         var secondUnit = playerTurnFirst ? enemyUnit : playerUnit;


//     //         var secondPokemon = secondUnit.BattlePokemon;

//     //         //선턴
//     //         yield return RunSkill(firstUnit, secondUnit, firstUnit.BattlePokemon.CurrentSkill);
//     //         yield return RunAfterTrun(firstUnit);
//     //         // if (state == BattleState.BattleOver)
//     //         // {
//     //         //     yield break;
//     //         // }
//     //         if (secondUnit.BattlePokemon.PokemonHp <= 0)
//     //         {
//     //             yield return HandlePokemonFainted(secondUnit);
//     //             yield return CheckForBattleOver(secondUnit);
//     //             yield break;
//     //         }
//     //         if (secondPokemon.PokemonHp > 0)
//     //         {
//     //             //후턴
//     //             yield return RunSkill(secondUnit, firstUnit, secondUnit.BattlePokemon.CurrentSkill);
//     //             yield return RunAfterTrun(secondUnit);
//     //             if (state == BattleState.BattleOver)
//     //             {
//     //                 yield break;
//     //             }
//     //         }
//     //         yield return RunSkill(secondUnit, firstUnit, secondUnit.BattlePokemon.CurrentSkill);
//     //         yield return RunAfterTrun(secondUnit);

//     //         if (firstUnit.BattlePokemon.PokemonHp <= 0)
//     //         {
//     //             yield return HandlePokemonFainted(firstUnit);
//     //             yield return CheckForBattleOver(firstUnit);
//     //             yield break;
//     //         }
//     //     }
//     //     else
//     //     {
//     //         if (playerAction == BattleAction.SwitchPokemon)
//     //         {
//     //             var selectedPokemon = playerParty.Party[currentMember];
//     //             state = BattleState.Busy;
//     //             yield return SwitchPokemon(selectedPokemon);
//     //         }
//     //         else if (playerAction == BattleAction.UseItem)
//     //         {
//     //             dialogBox.EnableActionSelector(false);
//     //             yield return ThrowPokeball();
//     //         }
//     //         else if (playerAction == BattleAction.Run)
//     //         {
//     //             yield return TryToRun();
//     //         }

//     //         var enemySkill = enemyUnit.BattlePokemon.GetRandomSkill();
//     //         yield return RunSkill(enemyUnit, playerUnit, enemySkill);
//     //         yield return RunAfterTrun(enemyUnit);
//     //         if (state == BattleState.BattleOver)
//     //         {
//     //             yield break;
//     //         }
//     //     }

//     //     if (state != BattleState.BattleOver)
//     //     {
//     //         ActionSelection();
//     //     }
//     // }

//     // IEnumerator RunTurns(BattleAction playerAction)
//     // {
//     //     state = BattleState.RunningTurn;
//     //     if (playerAction == BattleAction.Skill)
//     //     {
//     //         playerUnit.BattlePokemon.CurrentSkill = playerUnit.BattlePokemon.Skills[currentSkill];
//     //         enemyUnit.BattlePokemon.CurrentSkill = enemyUnit.BattlePokemon.GetRandomSkill();

//     //         if (playerUnit.BattlePokemon.CurrentSkill == null || enemyUnit.BattlePokemon.CurrentSkill == null)
//     //         {
//     //             Debug.LogError("❌ 스킬이 설정되지 않았습니다!");
//     //             yield break;
//     //         }

//     //         int playerPriority = playerUnit.BattlePokemon.CurrentSkill.SkillBase.Priority;
//     //         int enemyPriority = enemyUnit.BattlePokemon.CurrentSkill.SkillBase.Priority;

//     //         bool playerTurnFirst;
//     //         if (enemyPriority > playerPriority)
//     //             playerTurnFirst = false;
//     //         else if (playerPriority > enemyPriority)
//     //             playerTurnFirst = true;
//     //         else
//     //             playerTurnFirst = playerUnit.BattlePokemon.Speed == enemyUnit.BattlePokemon.Speed
//     //                 ? UnityEngine.Random.Range(0, 2) == 0
//     //                 : playerUnit.BattlePokemon.Speed > enemyUnit.BattlePokemon.Speed;

//     //         var firstUnit = playerTurnFirst ? playerUnit : enemyUnit;
//     //         var secondUnit = playerTurnFirst ? enemyUnit : playerUnit;

//     //         // 선턴
//     //         yield return RunSkill(firstUnit, secondUnit, firstUnit.BattlePokemon.CurrentSkill);
//     //         yield return RunAfterTrun(firstUnit);
//     //         if (secondUnit.BattlePokemon.PokemonHp <= 0)
//     //         {
//     //             yield return HandlePokemonFainted(secondUnit);
//     //             yield return CheckForBattleOver(secondUnit);
//     //             yield break;
//     //         }

//     //         // 후턴
//     //         if (secondUnit.BattlePokemon.PokemonHp > 0)
//     //         {
//     //             yield return RunSkill(secondUnit, firstUnit, secondUnit.BattlePokemon.CurrentSkill);
//     //             yield return RunAfterTrun(secondUnit);
//     //             if (firstUnit.BattlePokemon.PokemonHp <= 0)
//     //             {
//     //                 yield return HandlePokemonFainted(firstUnit);
//     //                 yield return CheckForBattleOver(firstUnit);
//     //                 yield break;
//     //             }
//     //         }
//     //     }
//     //     else
//     //     {
//     //         if (playerAction == BattleAction.SwitchPokemon)
//     //         {
//     //             var selectedPokemon = playerParty.Party[currentMember];
//     //             state = BattleState.Busy;
//     //             yield return SwitchPokemon(selectedPokemon);
//     //         }
//     //         else if (playerAction == BattleAction.UseItem)
//     //         {
//     //             dialogBox.EnableActionSelector(false);
//     //             yield return ThrowPokeball();
//     //         }
//     //         else if (playerAction == BattleAction.Run)
//     //         {
//     //             yield return TryToRun();
//     //         }

//     //         if (state != BattleState.BattleOver)
//     //         {
//     //             var enemySkill = enemyUnit.BattlePokemon.GetRandomSkill();
//     //             if (enemySkill != null)
//     //             {
//     //                 yield return RunSkill(enemyUnit, playerUnit, enemySkill);
//     //                 yield return RunAfterTrun(enemyUnit);
//     //             }
//     //         }
//     //     }

//     //     if (state != BattleState.BattleOver)
//     //     {
//     //         ActionSelection();
//     //     }
//     // }

//     IEnumerator RunTurns(BattleAction playerAction)
//     {
//         Debug.Log("🌀 RunTurns 시작");
//         state = BattleState.RunningTurn;

//         if (playerAction == BattleAction.Skill)
//         {
//             Debug.Log("▶ 플레이어가 Skill을 선택함");

//             playerUnit.BattlePokemon.CurrentSkill = playerUnit.BattlePokemon.Skills[currentSkill];
//             enemyUnit.BattlePokemon.CurrentSkill = enemyUnit.BattlePokemon.GetRandomSkill();

//             if (playerUnit.BattlePokemon.CurrentSkill == null || enemyUnit.BattlePokemon.CurrentSkill == null)
//             {
//                 Debug.LogError("❌ CurrentSkill이 null입니다. 스킬 설정 실패!");
//                 yield break;
//             }

//             int playerPriority = playerUnit.BattlePokemon.CurrentSkill.SkillBase.Priority;
//             int enemyPriority = enemyUnit.BattlePokemon.CurrentSkill.SkillBase.Priority;

//             bool playerTurnFirst = true;
//             if (enemyPriority > playerPriority)
//             {
//                 playerTurnFirst = false;
//             }
//             else if (playerPriority == enemyPriority)
//             {
//                 if (playerUnit.BattlePokemon.Speed == enemyUnit.BattlePokemon.Speed)
//                 {
//                     playerTurnFirst = UnityEngine.Random.Range(0, 2) == 0;
//                 }
//                 else
//                 {
//                     playerTurnFirst = playerUnit.BattlePokemon.Speed > enemyUnit.BattlePokemon.Speed;
//                 }
//             }

//             var firstUnit = playerTurnFirst ? playerUnit : enemyUnit;
//             var secondUnit = playerTurnFirst ? enemyUnit : playerUnit;

//             var targetOfFirst = secondUnit;
//             var targetOfSecond = firstUnit;

//             Debug.Log($"🎯 선공자: {(firstUnit == playerUnit ? "플레이어" : "상대")}");
//             Debug.Log($"🛡️ 후공자: {(secondUnit == playerUnit ? "플레이어" : "상대")}");

//             // 1. 선공자 행동
//             yield return RunSkill(firstUnit, targetOfFirst, firstUnit.BattlePokemon.CurrentSkill);
//             yield return RunAfterTrun(firstUnit);

//             if (state == BattleState.BattleOver)
//             {
//                 Debug.Log("🏁 선공자 행동 후 전투 종료됨");
//                 yield break;
//             }

//             // 선공자가 공격한 대상 쓰러짐 확인
//             if (targetOfFirst.BattlePokemon == null || targetOfFirst.BattlePokemon.PokemonHp <= 0)
//             {
//                 Debug.Log("⚠️ 선공자가 공격한 대상 쓰러짐");
//                 yield return HandlePokemonFainted(targetOfFirst);
//                 yield return CheckForBattleOver(targetOfFirst);
//                 yield break;
//             }

//             // 2. 후공자 행동 (자기와 대상 모두 살아 있을 때만)
//             if (
//                 secondUnit.BattlePokemon != null &&
//                 secondUnit.BattlePokemon.PokemonHp > 0 &&
//                 targetOfSecond.BattlePokemon != null &&
//                 targetOfSecond.BattlePokemon.PokemonHp > 0
//             )
//             {
//                 Debug.Log("🎮 후공자 행동 시작");
//                 yield return RunSkill(secondUnit, targetOfSecond, secondUnit.BattlePokemon.CurrentSkill);
//                 yield return RunAfterTrun(secondUnit);

//                 if (state == BattleState.BattleOver)
//                 {
//                     Debug.Log("🏁 후공자 행동 후 전투 종료됨");
//                     yield break;
//                 }

//                 if (targetOfSecond.BattlePokemon != null && targetOfSecond.BattlePokemon.PokemonHp <= 0)
//                 {
//                     Debug.Log("⚠️ 후공자가 공격한 대상 쓰러짐");
//                     yield return HandlePokemonFainted(targetOfSecond);
//                     yield return CheckForBattleOver(targetOfSecond);
//                     yield break;
//                 }
//             }
//             else
//             {
//                 Debug.Log("⛔ 후공자 또는 대상이 쓰러진 상태. 후공 행동 생략.");
//             }
//         }
//         else
//         {
//             Debug.Log($"▶ 플레이어가 {playerAction} 선택");

//             if (playerAction == BattleAction.SwitchPokemon)
//             {
//                 var selectedPokemon = playerParty.Party[currentMember];
//                 state = BattleState.Busy;
//                 yield return SwitchPokemon(selectedPokemon);
//             }
//             else if (playerAction == BattleAction.UseItem)
//             {
//                 dialogBox.EnableActionSelector(false);
//                 yield return ThrowPokeball();
//             }
//             else if (playerAction == BattleAction.Run)
//             {
//                 yield return TryToRun();
//             }

//             if (state == BattleState.BattleOver)
//             {
//                 Debug.Log("🏁 아이템/교체/도망 후 전투 종료됨");
//                 yield break;
//             }

//             // 적 턴
//             Debug.Log("👾 적 턴 시작");
//             var enemySkill = enemyUnit.BattlePokemon.GetRandomSkill();

//             if (enemyUnit.BattlePokemon == null || enemyUnit.BattlePokemon.PokemonHp <= 0)
//             {
//                 Debug.LogWarning("❗ 적 포켓몬이 쓰러졌음. 적 턴 스킵.");
//             }
//             else
//             {
//                 yield return RunSkill(enemyUnit, playerUnit, enemySkill);
//                 yield return RunAfterTrun(enemyUnit);

//                 if (state == BattleState.BattleOver)
//                 {
//                     Debug.Log("🏁 적 턴 종료 후 전투 종료됨");
//                 }

//                 if (playerUnit.BattlePokemon.PokemonHp <= 0)
//                 {
//                     Debug.Log("⚠️ 플레이어 포켓몬 쓰러짐");
//                     yield return HandlePokemonFainted(playerUnit);
//                     yield return CheckForBattleOver(playerUnit);
//                     yield break;
//                 }
//             }
//         }

//         if (state != BattleState.BattleOver)
//         {
//             Debug.Log("🔁 다음 턴 선택창으로 이동: ActionSelection()");
//             ActionSelection();
//         }

//         Debug.Log("✅ RunTurns 종료");
//     }




//     IEnumerator RunSkill(BattleUnit sourceUnit, BattleUnit targetUnit, Skill skill)
//     {
//         bool canRunSkill = sourceUnit.BattlePokemon.OnBeforeSkill();
//         if (canRunSkill == false)
//         {
//             yield return ShowStatusChanges(sourceUnit.BattlePokemon);
//             yield return sourceUnit.BattleHud.UpdateHp();
//             yield break;
//         }
//         yield return ShowStatusChanges(sourceUnit.BattlePokemon);
//         /*
//         // 모든 스킬의 PP가 0인지 확인
//         if (sourceUnit.BattlePokemon.Skills.TrueForAll(s => s.SkillPP <= 0))
//         {
//             // "발버둥" 기본 기술 사용
//             yield return dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.PokemonBase.PokemonName}은(는) 사용할 스킬이 없습니다! 발버둥을 사용합니다!");
//             skill = struggleSkill; // 발버둥 기술로 대체
//         }
//         */

//         // if (skill.SkillPP <= 0)
//         // {
//         //     // 행동 선택 상태로 복귀
//         //     if (sourceUnit.IsPlayerUnit)
//         //     {
//         //         // 스킬 사용 불가 메시지 출력
//         //         yield return dialogBox.TypeDialog($"기술의 남은 포인트가 없다!");
//         //         ActionSelection();
//         //     }
//         //     yield break; // 현재 실행 종료
//         // }

//         skill.PP--;

//         yield return dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.P_Base.PokemonName}의 {skill.SkillBase.SkillName}!");

//         //공격 애니메이션

//         //피격 애니메이션

//         if (CheckSkillHits(skill, sourceUnit.BattlePokemon, targetUnit.BattlePokemon))
//         {
//             if (skill.SkillBase.CategoryKey == CategoryKey.Status)
//             {
//                 if (targetUnit.BattlePokemon.Status != null)
//                 {
//                     yield return dialogBox.TypeDialog("효과가 없는 것 같다...");
//                 }
//                 else
//                 {
//                     yield return RunSkillEffects(skill.SkillBase.Effects, sourceUnit.BattlePokemon, targetUnit.BattlePokemon, skill.SkillBase.Target);
//                 }
//             }
//             else
//             {
//                 var (startHp, endHp, damageDetails) = targetUnit.BattlePokemon.TakeDamage(skill, sourceUnit.BattlePokemon);

//                 StartCoroutine(targetUnit.BattleHud.UpdateHp());
//                 yield return ShowDamageDetails(damageDetails);
//             }
//             if (skill.SkillBase.SecondaryEffects != null && skill.SkillBase.SecondaryEffects.Count > 0 && targetUnit.BattlePokemon.PokemonHp > 0)
//             {
//                 foreach (var secondary in skill.SkillBase.SecondaryEffects)
//                 {
//                     var rnd = UnityEngine.Random.Range(1, 101);
//                     if (rnd <= secondary.Chance)
//                     {
//                         yield return RunSkillEffects(secondary, sourceUnit.BattlePokemon, targetUnit.BattlePokemon, secondary.Target);
//                     }
//                 }
//             }

//             if (targetUnit.BattlePokemon.PokemonHp <= 0)
//             {
//                 yield return HandlePokemonFainted(targetUnit);
//             }
//         }
//         else
//         {
//             if (sourceUnit.IsPlayerUnit)
//             {
//                 yield return dialogBox.TypeDialog($"상대 {targetUnit.BattlePokemon.P_Base.PokemonName}에게는 \n맞지 않았다!");
//             }
//             else
//             {
//                 yield return dialogBox.TypeDialog($"{targetUnit.BattlePokemon.P_Base.PokemonName}에게는 맞지 않았다!");
//             }
//         }
//     }
//     IEnumerator RunAfterTrun(BattleUnit sourceUnit)
//     {
//         if (state == BattleState.BattleOver)
//         {
//             yield break;
//         }
//         yield return new WaitUntil(() => state == BattleState.RunningTurn);
//         yield return new WaitForSeconds(1.2f);
//         //상태이상 처리
//         sourceUnit.BattlePokemon.OnAfterTurn();
//         yield return ShowStatusChanges(sourceUnit.BattlePokemon);
//         yield return sourceUnit.BattleHud.UpdateHp();
//         if (sourceUnit.BattlePokemon.PokemonHp <= 0)
//         {
//             yield return HandlePokemonFainted(sourceUnit);
//             yield return new WaitUntil(() => state == BattleState.RunningTurn);
//             // yield return dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.PokemonBase.PokemonName}{GetCorrectParticle(sourceUnit.BattlePokemon.PokemonBase.PokemonName, false)} 쓰러졌다!");
//             // /*
//             // 사망 애니메이션 재생

//             // */

//             // yield return new WaitForSeconds(2.0f);
//         }
//     }
//     IEnumerator RunSkillEffects(SkillEffects effects, Pokemon sourceUnit, Pokemon targetUnit, SkillTarget skillTarget)
//     {
//         //RankUp
//         if (effects.Rankup != null)
//         {
//             if (skillTarget == SkillTarget.Self)
//             {
//                 sourceUnit.ApplyRankups(effects.Rankup);
//             }
//             else
//             {
//                 targetUnit.ApplyRankups(effects.Rankup);
//             }
//         }
//         //상태이상
//         if (effects.Status != ConditionID.None)
//         {
//             targetUnit.SetStatus(effects.Status);
//         }
//         //일시 상태이상
//         if (effects.VolatileStatus != ConditionID.None)
//         {
//             targetUnit.SetVolatileStatus(effects.VolatileStatus);
//         }

//         yield return ShowStatusChanges(sourceUnit);
//         yield return ShowStatusChanges(targetUnit);
//     }
//     bool CheckSkillHits(Skill skill, Pokemon source, Pokemon target)
//     {
//         if (skill.SkillBase.AlwaysHits)
//         {
//             return true;
//         }
//         float SkillAccuracy = skill.SkillBase.SkillAccuracy;
//         int accuracy = source.Rankup[Stat.Accuracy];
//         int evasion = target.Rankup[Stat.Evasion];

//         var rankupValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

//         if (accuracy > 0)
//         {
//             SkillAccuracy *= rankupValues[accuracy];
//         }
//         else
//         {
//             SkillAccuracy /= rankupValues[-accuracy];
//         }
//         if (evasion > 0)
//         {
//             SkillAccuracy /= rankupValues[evasion];
//         }
//         else
//         {
//             SkillAccuracy *= rankupValues[-evasion];
//         }


//         return UnityEngine.Random.Range(1, 101) <= SkillAccuracy;
//     }
//     IEnumerator ShowStatusChanges(Pokemon pokemon)
//     {
//         while (pokemon.StatusCngMsg.Count > 0)
//         {
//             string message = pokemon.StatusCngMsg.Dequeue();
//             yield return dialogBox.TypeDialog(message);
//         }
//     }
//     IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
//     {
//         yield return dialogBox.TypeDialog($"{faintedUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(faintedUnit.BattlePokemon.P_Base.PokemonName, "topic")} 쓰러졌다!");
//         //애니메이션 재생

//         //플레이어 승리
//         yield return new WaitForSeconds(1.5f);

//         if (!faintedUnit.IsPlayerUnit)
//         {
//             int expYield = faintedUnit.BattlePokemon.P_Base.ExpYield;
//             int enemyLevel = faintedUnit.BattlePokemon.PokemonLevel;
//             float trainerBonus = (isTrainerBattle) ? 1.5f : 1.0f;

//             int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus / 7);
//             playerUnit.BattlePokemon.PokemonExp += expGain;
//             yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")}\n{expGain}경험치를 얻었다!");
//             yield return playerUnit.BattleHud.SetExpSmooth();
//             yield return CheckLearnableSkill();
//         }
//         yield return CheckForBattleOver(faintedUnit);
//         GameManager.Inst.AddGold();
//     }

//     bool cancelSelected = false;
//     IEnumerator CheckLearnableSkill()
//     {
//         while (playerUnit.BattlePokemon.CheckForLevelUp())
//         {
//             playerUnit.BattleHud.SetLevel();
//             yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 레벨 {playerUnit.BattlePokemon.PokemonLevel}로 올랐다!");

//             var newSkill = playerUnit.BattlePokemon.GetLearnableSkill();
//             if (newSkill != null)
//             {
//                 if (playerUnit.BattlePokemon.Skills.Count < PokemonBase.MaxNumOfSkills)
//                 {
//                     playerUnit.BattlePokemon.LearnSkill(newSkill);
//                     yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 새로 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배웠다!");
//                     dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);
//                 }
//                 else
//                 {
//                     bool isFinalDecisionMade = false;
//                     while (!isFinalDecisionMade)
//                     {
//                         yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 새로 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우고 싶어한다!");
//                         yield return dialogBox.TypeDialog($"하지만 기술이 4개이므로 다른 기술을 잊어야 한다.");
//                         yield return dialogBox.TypeDialog($"{newSkill.SkillBase.SkillName} 대신 다른 기술을 잊게 하겠습니까?");

//                         ConfirmBoxSelection();
//                         yield return new WaitUntil(() => state != BattleState.ConfirmBox);
//                         bool isConfirmed = HandleConfirmBoxSelection();

//                         if (isConfirmed)
//                         {
//                             yield return ChooseSkillToForget(playerUnit.BattlePokemon, newSkill.SkillBase);
//                             yield return new WaitUntil(() => state == BattleState.Busy);

//                             if (cancelSelected || currentSelection == PokemonBase.MaxNumOfSkills)
//                             {
//                                 cancelSelected = false;

//                                 yield return dialogBox.TypeDialog($"그럼... {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우는 것을 포기하겠습니까?");
//                                 ConfirmBoxSelection();
//                                 yield return new WaitUntil(() => state != BattleState.ConfirmBox);
//                                 bool giveUp = HandleConfirmBoxSelection();

//                                 if (giveUp)
//                                 {
//                                     yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 결국 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우지 않았다!");
//                                     skillToLearn = null;
//                                     isFinalDecisionMade = true;
//                                 }
//                                 else
//                                 {
//                                     yield return ChooseSkillToForget(playerUnit.BattlePokemon, newSkill.SkillBase);
//                                     yield return new WaitUntil(() => state == BattleState.SkillToForget);
//                                 }
//                             }
//                             else
//                             {
//                                 var oldSkill = playerUnit.BattlePokemon.Skills[currentSelection].SkillBase;
//                                 playerUnit.BattlePokemon.Skills[currentSelection] = new Skill(newSkill.SkillBase);

//                                 yield return dialogBox.TypeDialog("1, 2, ... 짠!");
//                                 yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} {oldSkill.SkillName}{GetCorrectParticle(oldSkill.SkillName, "object")} 깨끗이 잊었다!");
//                                 yield return dialogBox.TypeDialog($"그리고 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배웠다!");

//                                 dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);
//                                 skillToLearn = null;
//                                 isFinalDecisionMade = true;
//                             }
//                         }
//                         else
//                         {
//                             yield return dialogBox.TypeDialog($"그럼... {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우는 것을 포기하겠습니까?");
//                             ConfirmBoxSelection();
//                             yield return new WaitUntil(() => state != BattleState.ConfirmBox);
//                             bool isReallyConfirmed = HandleConfirmBoxSelection();

//                             if (isReallyConfirmed)
//                             {
//                                 yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 결국 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우지 않았다!");
//                                 skillToLearn = null;
//                                 isFinalDecisionMade = true;
//                             }
//                         }
//                     }
//                 }
//             }

//             yield return playerUnit.BattleHud.SetExpSmooth(true);
//         }
//     }
//     IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
//     {
//         if (faintedUnit.IsPlayerUnit)
//         {
//             var nextPokemon = playerParty.GetHealthyPokemon();
//             if (nextPokemon != null)
//             {
//                 OpenPartyScreen();
//             }
//             else
//             {
//                 BattleOver(false);
//             }
//         }
//         else
//         {
//             if (!isTrainerBattle)
//             {
//                 yield return playerParty.CheckForEvolutions();
//                 yield return new WaitForSeconds(0.5f); // 진화 마무리 대기

//                 StopAllCoroutines();
//                 BattleOver(true);
//             }
//             else
//             {
//                 var nextPokemon = trainerParty.GetHealthyPokemon();
//                 if (nextPokemon != null)
//                 {
//                     //다음포케
//                     yield return playerParty.CheckForEvolutions();

//                 }
//                 else
//                 {
//                     yield return playerParty.CheckForEvolutions();

//                     BattleOver(true);
//                 }
//             }
//         }
//     }
//     IEnumerator ShowDamageDetails(DamageDetails damageDetails)
//     {
//         if (damageDetails.Critical > 1f)
//         {
//             yield return dialogBox.TypeDialog("급소에 맞았다!");
//         }
//         if (damageDetails.TypeEffectiveness > 1)
//         {
//             if (damageDetails.TypeEffectiveness > 2)
//             {
//                 yield return dialogBox.TypeDialog("효과가 굉장했다!!");
//             }
//             else
//             {
//                 yield return dialogBox.TypeDialog("효과가 대단했다!");
//             }
//         }

//         else if (damageDetails.TypeEffectiveness < 1f)
//         {
//             if (damageDetails.TypeEffectiveness == 0)
//             {
//                 yield return dialogBox.TypeDialog("효과가 없는 듯 하다...");
//             }
//             else
//             {
//                 yield return dialogBox.TypeDialog("효과가 별로인듯 하다...");
//             }
//         }
//     }
//     #endregion
//     void HandleActionSelection()
//     {
//         if (Input.GetKeyDown(KeyCode.DownArrow))
//         {
//             if (currentAction < 2)
//             {
//                 currentAction = currentAction + 2;
//             }
//         }
//         if (Input.GetKeyDown(KeyCode.RightArrow))
//         {
//             if (currentAction < 3)
//             {
//                 ++currentAction;
//             }
//         }
//         if (Input.GetKeyDown(KeyCode.UpArrow))
//         {
//             if (1 < currentAction)
//             {
//                 currentAction = currentAction - 2;
//             }
//         }
//         if (Input.GetKeyDown(KeyCode.LeftArrow))
//         {
//             if (0 < currentAction)
//             {
//                 --currentAction;
//             }
//         }
//         currentAction = Mathf.Clamp(currentAction, 0, 3);
//         dialogBox.UpdateActionSelection(currentAction);
//         if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
//         {
//             if (currentAction == 0)
//             {//싸운다
//                 SkillSelection();
//             }
//             else if (currentAction == 1)
//             {//볼
//                 StartCoroutine(RunTurns(BattleAction.UseItem));
//             }
//             else if (currentAction == 2)
//             {//포켓몬
//                 preState = state;
//                 OpenPartyScreen();
//                 // Debug.Log("포켓몬");
//             }
//             else if (currentAction == 3)
//             {//도망친다
//              // Debug.Log("도망");
//                 StartCoroutine(RunTurns(BattleAction.Run));
//             }
//         }
//     }
//     void HandleSkillSelection()
//     {
//         if (Input.GetKeyDown(KeyCode.DownArrow) && currentSkill + 2 < skillCount)
//         {
//             currentSkill += 2;
//         }
//         if (Input.GetKeyDown(KeyCode.RightArrow) && currentSkill + 1 < skillCount)
//         {
//             currentSkill++;
//         }
//         if (Input.GetKeyDown(KeyCode.UpArrow) && currentSkill - 2 >= 0)
//         {
//             currentSkill -= 2;
//         }
//         if (Input.GetKeyDown(KeyCode.LeftArrow) && currentSkill - 1 >= 0)
//         {
//             currentSkill--;
//         }

//         currentSkill = Mathf.Clamp(currentSkill, 0, skillCount - 1);

//         if (skillCount > 0)
//         {
//             dialogBox.UpdateSkillSelection(currentSkill, playerUnit.BattlePokemon.Skills[currentSkill]);
//         }

//         if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
//         {
//             var skill = playerUnit.BattlePokemon.Skills[currentSkill];
//             if (skill.PP == 0)
//             {
//                 // 스킬 사용 불가 메시지 출력
//                 dialogBox.EnableSkillSelector(false);
//                 dialogBox.EnableDialogText(true);
//                 StartCoroutine(dialogBox.TypeDialog($"기술의 남은 포인트가 없다!"));
//                 ActionSelection();
//                 return;
//             }

//             dialogBox.EnableSkillSelector(false);
//             dialogBox.EnableDialogText(true);
//             StartCoroutine(RunTurns(BattleAction.Skill));
//             // Debug.Log($"playerUnit.BattlePokemon: {playerUnit.BattlePokemon?.P_Base?.PokemonName}");
//             // Debug.Log($"Skill[0]: {playerUnit.BattlePokemon?.Skills[0]?.SkillBase?.SkillName}");
//         }
//         else if (Input.GetKeyDown(KeyCode.Backspace))
//         {
//             dialogBox.EnableSkillSelector(false);
//             dialogBox.EnableDialogText(true);
//             ActionSelection();
//         }
//     }
//     #region PartySystem
//     void HandlePartyScreenSelection()
//     {
//         if (Input.GetKeyDown(KeyCode.DownArrow))
//         {
//             currentMember++;
//         }
//         if (Input.GetKeyDown(KeyCode.RightArrow))
//         {
//             currentMember++;
//         }
//         if (Input.GetKeyDown(KeyCode.UpArrow))
//         {
//             currentMember--;
//         }
//         if (Input.GetKeyDown(KeyCode.LeftArrow))
//         {
//             currentMember = 0;
//         }
//         currentMember = Mathf.Clamp(currentMember, 0, playerParty.Party.Count - 1);
//         partyScreen.UpdateMemberSelection(currentMember);
//         if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
//         {
//             //포켓몬 교체
//             var selectedMember = playerParty.Party[currentMember];
//             if (selectedMember.PokemonHp <= 0)
//             {
//                 partyScreen.SetMessageText($"{playerParty.Party[currentMember].P_Base.PokemonName}{GetCorrectParticle(playerParty.Party[currentMember].P_Base.PokemonName, "topic")} 싸울 수 있는 \n기력이 남아 있지 않습니다!");
//                 return;
//             }
//             if (selectedMember == playerUnit.BattlePokemon)
//             {
//                 //능력치보기, 놓아주기, 그만두기 구현
//                 partyScreen.SetMessageText($"이미 전투 중인 포켓몬으로 교체 할 수 없습니다!");
//                 return;
//             }

//             partyScreen.gameObject.SetActive(false);

//             if (preState == BattleState.ActionSelection)
//             {
//                 preState = null;
//                 StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
//                 dialogBox.EnableActionSelector(false);
//             }
//             //포켓몬이 쓰러졌을때
//             else
//             {
//                 state = BattleState.Busy;
//                 StartCoroutine(SwitchPokemon(selectedMember));
//             }
//         }
//         else if (Input.GetKeyDown(KeyCode.Backspace))
//         {
//             partyScreen.gameObject.SetActive(false);
//             ActionSelection();
//         }
//     }
//     IEnumerator SwitchPokemon(Pokemon newPokemon)
//     {
//         playerUnit.BattlePokemon.CureVolatileStatus();
//         playerUnit.BattlePokemon.ResetRankup();

//         yield return dialogBox.TypeDialog($"돌아와 {playerUnit.BattlePokemon.P_Base.PokemonName}!");
//         //사망애니메이션
//         yield return new WaitForSeconds(1.5f);

//         // 현재 전투 중인 포켓몬 (인덱스 0에 있는 포켓몬)
//         var currentBattlePokemon = playerParty.Party[0];

//         // 교체 작업: 교체할 포켓몬을 0번 인덱스로, 나가있는 포켓몬을 교체할 포켓몬의 인덱스로 이동
//         playerParty.Party[0] = newPokemon;
//         playerParty.Party[currentMember] = currentBattlePokemon;

//         playerUnit.SetUp(newPokemon);
//         dialogBox.SetSkillNames(newPokemon.Skills);

//         skillCount = newPokemon.Skills.Count;

//         yield return dialogBox.TypeDialog($"가랏! {newPokemon.P_Base.PokemonName}!");
//         state = BattleState.RunningTurn;
//     }
//     #endregion
//     #region LearnSkill
//     // public void HandleLearnSkillSelection()
//     // {
//     //     if (Input.GetKeyDown(KeyCode.DownArrow))
//     //     {
//     //         currentSelection++;
//     //     }
//     //     else if (Input.GetKeyDown(KeyCode.UpArrow))
//     //     {
//     //         currentSelection--;
//     //     }
//     //     currentSelection = Mathf.Clamp(currentSelection, 0, PokemonBase.MaxNumOfSkills);
//     //     skillSelectScreen.UpdateSkillSelection(currentSelection);

//     //     if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
//     //     {
//     //         {
//     //             skillSelectScreen.gameObject.SetActive(false);
//     //             if (currentSelection == PokemonBase.MaxNumOfSkills)
//     //             {
//     //                 //배우지않음
//     //                 if (currentSelection == PokemonBase.MaxNumOfSkills)
//     //                 {
//     //                     //배우지않음
//     //                     //그럼... {}을
//     //                     //배우는 것을 포기하겠습니까?
//     //                     //예/아니오
//     //                 }
//     //                 state = BattleState.RunningTurn;
//     //             }
//     //             else
//     //             {
//     //                 var selectedSkill = playerUnit.BattlePokemon.Skills[currentSelection].SkillBase;
//     //                 playerUnit.BattlePokemon.Skills[currentSelection] = new Skill(skillToLearn);
//     //                 IEnumerator TypeText()
//     //                 {
//     //                     yield return dialogBox.TypeDialog("1, 2, ... ... 짠!");
//     //                     yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} {selectedSkill.SkillName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "object")} 깨끗이 잊었다!");
//     //                     yield return dialogBox.TypeDialog("그리고...");
//     //                     yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 새로\n{playerUnit.BattlePokemon.Skills[currentSelection].SkillBase.SkillName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "object")} 배웠다!");
//     //                 }
//     //                 StartCoroutine(TypeText());
//     //             }
//     //             skillToLearn = null;
//     //             state = BattleState.RunningTurn;
//     //         }
//     //     }
//     //     else if (Input.GetKeyDown(KeyCode.Backspace))
//     //     {
//     //         skillSelectScreen.gameObject.SetActive(false);



//     //         // ConfirmBoxSelection();
//     //         // yield return new WaitUntil(() => state != BattleState.ConfirmBox);
//     //         // bool isConfirmed = HandleConfirmBoxSelection();
//     //         // if (isConfirmed)
//     //         // {
//     //         //     yield return ChooseSkillToForget(playerUnit.BattlePokemon, newSkill.SkillBase);
//     //         //     yield return new WaitUntil(() => state != BattleState.SkillToForget);
//     //         //     yield return new WaitForSeconds(5.0f);
//     //         //     isFinalDecisionMade = true;
//     //         // }
//     //         // else
//     //         // {
//     //         //     yield return dialogBox.TypeDialog($"그럼... {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")}\n배우는 것을 포기하겠습니까?");
//     //         //     ConfirmBoxSelection();
//     //         //     yield return new WaitUntil(() => state != BattleState.ConfirmBox);
//     //         //     bool isRealConfirmed = HandleConfirmBoxSelection();
//     //         //     if (isRealConfirmed)
//     //         //     {
//     //         //         yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")}{newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")}\n결국 배우지 않았다!");
//     //         //         yield return new WaitForSeconds(1.0f);
//     //         //         isFinalDecisionMade = true;
//     //         //     }
//     //         // }
//     //         state = BattleState.RunningTurn;
//     //     }
//     // }

//     public void HandleLearnSkillSelection()
//     {
//         if (Input.GetKeyDown(KeyCode.DownArrow))
//         {
//             currentSelection++;
//         }
//         else if (Input.GetKeyDown(KeyCode.UpArrow))
//         {
//             currentSelection--;
//         }

//         currentSelection = Mathf.Clamp(currentSelection, 0, PokemonBase.MaxNumOfSkills);
//         skillSelectScreen.UpdateSkillSelection(currentSelection);

//         if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
//         {
//             skillSelectScreen.gameObject.SetActive(false);
//             state = BattleState.Busy;
//         }
//         else if (Input.GetKeyDown(KeyCode.Backspace))
//         {
//             skillSelectScreen.gameObject.SetActive(false);
//             cancelSelected = true;
//             state = BattleState.Busy;
//         }
//     }
//     #endregion
//     #region  Confirm Box
//     public bool HandleConfirmBoxSelection()
//     {
//         if (Input.GetKeyDown(KeyCode.DownArrow))
//         {
//             currentConfirm++;
//         }
//         else if (Input.GetKeyDown(KeyCode.UpArrow))
//         {
//             currentConfirm--;
//         }
//         currentConfirm = Mathf.Clamp(currentConfirm, 0, 1);
//         dialogBox.ConfirmBoxSelection(currentConfirm);
//         if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
//         {
//             {
//                 if (currentConfirm == 0)
//                 {
//                     ConfirmBox.SetActive(false);

//                     state = BattleState.Busy;
//                     return true;
//                 }
//                 else
//                 {
//                     ConfirmBox.SetActive(false);

//                     state = BattleState.Busy;
//                     return false;
//                 }
//             }
//         }
//         else if (Input.GetKeyDown(KeyCode.Backspace))
//         {
//             ConfirmBox.SetActive(false);

//             state = BattleState.Busy;
//             return false;
//         }
//         else
//         {
//             return true;
//         }
//     }
//     #endregion
//     #region Config
//     void HandleConfigSelection()
//     {
//         if (configPanel.state == ConfigState.Config_Right)
//         {
//             if (Input.GetKeyDown(KeyCode.UpArrow))
//             {
//                 --currentConfig;
//             }
//             if (Input.GetKeyDown(KeyCode.DownArrow))
//             {
//                 ++currentConfig;
//             }
//             currentConfig = Mathf.Clamp(currentConfig, 0, 5);
//             configPanel.ConfigSelection(currentConfig);
//             if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
//             {
//                 if (currentConfig == 0)
//                 {//게임설정
//                     configPanel.SettingSelection();
//                 }
//                 else if (currentConfig == 1)
//                 {//도감
//                     Debug.Log("도감");
//                 }
//                 else if (currentConfig == 2)
//                 {//데이터관리
//                     Debug.Log("데이터 관리");
//                 }
//                 else if (currentConfig == 3)
//                 {//커뮤니티
//                     Debug.Log("커뮤니티");
//                 }
//                 else if (currentConfig == 4)
//                 {//저장 후 나가기
//                     var savingSystem = FindObjectOfType<SavingSystem>();
//                     if (savingSystem != null)
//                     {
//                         savingSystem.SaveGame();
//                         playerParty.Party.Clear();
//                         // Debug.Log("저장 완료!");
//                         SceneManager.LoadScene("LobbyScene");
//                         // Debug.Log("저장 후 나가기");
//                     }
//                     else
//                     {
//                         Debug.LogWarning("SavingSystem을 찾지 못했어요… 저장 실패!");
//                     }
//                 }
//                 else if (currentConfig == 5)
//                 {//로그아웃
//                     Debug.Log("로그아웃");
//                 }
//             }
//         }
//     }
//     #endregion
//     #region Catch
//     IEnumerator ThrowPokeball()
//     {
//         state = BattleState.Busy;

//         if (isTrainerBattle)
//         {
//             yield return dialogBox.TypeDialog("다른 트레이너의 포켓몬은 잡을 수 없다!");
//             state = BattleState.RunningTurn;
//             yield break;
//         }

//         var pokeballObj = Instantiate(Pokeball, playerUnit.transform.position, Quaternion.identity);
//         var pokeball = pokeballObj.GetComponent<SpriteRenderer>();

//         //#34 1254
//         //pokeball.transform.DoMove

//         int shakeCount = TryToCatchPokemon(enemyUnit.BattlePokemon);
//         for (int i = 0; i < Math.Min(shakeCount, 3); ++i)
//         {
//             yield return new WaitForSeconds(0.5f);
//             //흔들기 애니메이션
//         }
//         if (shakeCount == 4)
//         {
//             //잡힘
//             playerParty.AddPokemon(enemyUnit.BattlePokemon);

//             GlobalValue.CatchPokemon(enemyUnit.BattlePokemon.P_Base, false);

//             Destroy(pokeball);
//             yield return dialogBox.TypeDialog($"신난다-!\n야생 {enemyUnit.BattlePokemon.P_Base.PokemonName}을 잡았다!");
//             BattleOver(true);
//         }
//         else
//         {
//             yield return dialogBox.TypeDialog($"!");
//             Destroy(pokeball);
//             state = BattleState.RunningTurn;
//         }
//         yield return new WaitForSeconds(1.0f);
//     }
//     int TryToCatchPokemon(Pokemon pokemon)
//     {
//         float a = (3 * pokemon.MaxHp - 2 * pokemon.PokemonHp) * pokemon.P_Base.CatchRate * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

//         if (a >= 255)
//         {
//             //흔들린 횟수
//             return 4;
//         }

//         float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

//         int shakeCount = 0;
//         while (shakeCount < 4)
//         {
//             if (UnityEngine.Random.Range(0, 65535) >= b)
//             {
//                 break;
//             }
//             ++shakeCount;
//         }
//         return shakeCount;
//     }
//     #endregion

//     IEnumerator TryToRun()
//     {
//         state = BattleState.Busy;

//         if (isTrainerBattle)
//         {
//             yield return dialogBox.TypeDialog("");
//             state = BattleState.RunningTurn;
//             yield break;
//         }

//         ++escapeAttempts;

//         int playerSpeed = playerUnit.BattlePokemon.Speed;
//         int enemySpeed = enemyUnit.BattlePokemon.Speed;

//         if (enemySpeed <= playerSpeed)
//         {
//             yield return dialogBox.TypeDialog("무사히 도망쳤다!");
//             BattleOver(true);
//         }
//         else
//         {
//             float f = (playerSpeed * 128) / (enemySpeed + 30 * escapeAttempts);
//             f = f % 256;

//             if (UnityEngine.Random.Range(0, 256) < f)
//             {
//                 yield return dialogBox.TypeDialog("무사히 도망쳤다!");
//                 BattleOver(true);
//             }
//             else
//             {
//                 yield return dialogBox.TypeDialog("도망칠 수 없었다!");
//                 state = BattleState.RunningTurn;
//                 // yield break;

//             }
//         }
//     }


//     string GetCorrectParticle(string name, string particleType)    //은는이가
//     {
//         char lastChar = name[name.Length - 1];
//         int unicode = (int)lastChar;
//         bool endsWithConsonant = (unicode - 44032) % 28 != 0; // 44032는 '가'의 유니코드, 28는 받침의 수


//         switch (particleType)
//         {
//             case "subject": // 이/가
//                 { return endsWithConsonant ? "이" : "가"; }
//             case "topic": // 은/는
//                 { return endsWithConsonant ? "은" : "는"; }
//             case "object": // 을/를
//                 { return endsWithConsonant ? "을" : "를"; }
//             case "objectTo": // 로/으로
//                 { return endsWithConsonant ? "로" : "으로"; }
//             default:
//                 throw new ArgumentException("Invalid particle type");
//         }
//     }
// }
