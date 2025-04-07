// using System.Collections;
// using System;
// using UnityEngine;
// using System.Linq;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// using Cysharp.Threading.Tasks;
// using System.Threading.Tasks;

// public enum BattleState
// {
//     Start,
//     ActionSelection,
//     SkillSelection,
//     RunningTurn,
//     Busy,
//     Dialog,
//     PartyScreen,
//     SkillToForget,
//     ConfirmBox,
//     Evolution,
//     BattleOver,
//     ConfigSelection
// }
// public enum BattleAction
// {
//     Skill,
//     SwitchPokemon,
//     UseItem,
//     Run
// }
// public class BattleSystem : MonoBehaviour
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
//         Inst = this;
//         player = FindObjectOfType<PlayerCtrl>();
//     }
//     private void Start()
//     {
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
//         SetUpBattle();
//     }
//     // public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
//     // {
//     //     this.playerParty = playerParty;
//     //     this.trainerParty = trainerParty;

//     //     isTrainerBattle = true;

//     //     player = playerParty.GetComponent<PlayerCtrl>();
//     //     trainer = trainerParty.GetComponent<TrainerCtrl>();
//     //     UniTask.Void(async () =>
//     //        {
//     //            await SetUpBattle();
//     //        });
//     // }
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
//             Debug.Log(state);
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
//     public async UniTask SetUpBattle()
//     {
//         if (isTrainerBattle == false)
//         {
//             playerUnit.SetUp(playerParty.GetHealthyPokemon());
//             enemyUnit.SetUp(wildPokemon);

//             dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);
//             skillCount = playerUnit.BattlePokemon.Skills.Count;

//             await dialogBox.TypeDialog($"앗! 야생 {enemyUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(enemyUnit.BattlePokemon.P_Base.PokemonName, "subject")} \n튀어나왔다!");
//         }
//         else
//         {
//             playerUnit.gameObject.SetActive(false);
//             enemyUnit.gameObject.SetActive(false);

//             PlayerSprite.gameObject.SetActive(true);
//             TrainerSprite.gameObject.SetActive(true);

//             PlayerSprite.sprite = player.TrainerSprite;
//             TrainerSprite.sprite = trainer.TrainerSprite;

//             await dialogBox.TypeDialog($"{trainer.TrainerName}이 배틀을 걸어왔다!");
//         }

//         escapeAttempts = 0;
//         partyScreen.Init();
//         ActionSelection();
//     }

//     void BattleOver(bool won)
//     {
//         state = BattleState.BattleOver;
//         GameManager.Inst.EndBattle(won);
//     }
//     void ActionSelection()
//     {
//         state = BattleState.ActionSelection;
//         dialogBox.EnableActionSelector(true);
//         UniTask.Void(async () =>
//     {
//         await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 무엇을 할까?");
//     });
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
//     public async UniTask ChooseSkillToForget(Pokemon pokemon, SkillBase newSkill)
//     {
//         state = BattleState.Busy;

//         await dialogBox.TypeDialog("어느 기술을 잊게 하고싶은가?");

//         skillSelectScreen.gameObject.SetActive(true);
//         skillSelectScreen.SetPokemonData(pokemon);
//         skillSelectScreen.SetSkill(pokemon.Skills.Select(x => x.SkillBase).ToList(), newSkill);

//         skillToLearn = newSkill;

//         state = BattleState.SkillToForget;
//     }

//     #region BattleSystem
//     public async UniTask RunTurns(BattleAction playerAction)
//     {
//         Debug.Log($"[RunTurns] 시작: playerAction = {playerAction}");
//         state = BattleState.RunningTurn;

//         if (playerAction == BattleAction.Skill)
//         {
//             Debug.Log("[RunTurns] 플레이어: 스킬 선택");
//             playerUnit.BattlePokemon.CurrentSkill = playerUnit.BattlePokemon.Skills[currentSkill];
//             enemyUnit.BattlePokemon.CurrentSkill = enemyUnit.BattlePokemon.GetRandomSkill();

//             if (playerUnit.BattlePokemon.CurrentSkill == null || enemyUnit.BattlePokemon.CurrentSkill == null)
//             {
//                 return;
//             }

//             var (firstUnit, secondUnit) = GetTurnOrder(playerUnit, enemyUnit);

//             Debug.Log($"[RunTurns] 선공: {firstUnit.BattlePokemon.P_Base.PokemonName}");

//             await RunSkill(firstUnit, secondUnit, firstUnit.BattlePokemon.CurrentSkill);
//             await RunAfterTurn(firstUnit);

//             if (state == BattleState.BattleOver)
//             {
//                 return;
//             }

//             if (secondUnit.BattlePokemon?.PokemonHp <= 0)
//             {
//                 Debug.Log($"[RunTurns] 후공 {secondUnit.BattlePokemon.P_Base.PokemonName} 쓰러짐");
//                 await HandlePokemonFainted(secondUnit);
//                 await CheckForBattleOver(secondUnit);
//                 return;
//             }

//             if (secondUnit.BattlePokemon.PokemonHp > 0 && firstUnit.BattlePokemon.PokemonHp > 0)
//             {
//                 await RunSkill(secondUnit, firstUnit, secondUnit.BattlePokemon.CurrentSkill);
//                 await RunAfterTurn(secondUnit);

//                 if (firstUnit.BattlePokemon?.PokemonHp <= 0)
//                 {
//                     Debug.Log($"[RunTurns] 선공 {firstUnit.BattlePokemon.P_Base.PokemonName} 쓰러짐");
//                     await HandlePokemonFainted(firstUnit);
//                     await CheckForBattleOver(firstUnit);
//                     return;
//                 }
//             }
//         }
//         else
//         {
//             Debug.Log($"[RunTurns] 플레이어: {playerAction}");
//             await HandleNonSkillAction(playerAction);

//             if (state == BattleState.BattleOver)
//             {
//                 return;
//             }
//             if (playerAction == BattleAction.SwitchPokemon)
//             {
//                 Debug.Log("[RunTurns] 전술적 교체 → 적 턴 실행");
//                 if (enemyUnit.BattlePokemon.PokemonHp > 0)
//                 {
//                     var enemySkill = enemyUnit.BattlePokemon.GetRandomSkill();
//                     await RunSkill(enemyUnit, playerUnit, enemySkill);
//                     await RunAfterTurn(enemyUnit);

//                     if (playerUnit.BattlePokemon.PokemonHp <= 0)
//                     {
//                         Debug.Log("[RunTurns] 교체된 내 포켓몬 쓰러짐");
//                         await HandlePokemonFainted(playerUnit);
//                         await CheckForBattleOver(playerUnit);
//                         return;
//                     }
//                 }
//             }
//             if (enemyUnit.BattlePokemon.PokemonHp > 0)
//             {
//                 var enemySkill = enemyUnit.BattlePokemon.GetRandomSkill();
//                 await RunSkill(enemyUnit, playerUnit, enemySkill);
//                 await RunAfterTurn(enemyUnit);

//                 if (playerUnit.BattlePokemon.PokemonHp <= 0)
//                 {
//                     await HandlePokemonFainted(playerUnit);
//                     await CheckForBattleOver(playerUnit);
//                     return;
//                 }
//             }
//         }

//         if (state != BattleState.BattleOver)
//         {
//             Debug.Log("[RunTurns] 다음 턴으로 → ActionSelection()");
//             ActionSelection();
//         }
//         Debug.Log("[RunTurns] 종료");
//     }
//     (BattleUnit first, BattleUnit second) GetTurnOrder(BattleUnit player, BattleUnit enemy)
//     {
//         var playerSkill = player.BattlePokemon.CurrentSkill;
//         var enemySkill = enemy.BattlePokemon.CurrentSkill;

//         int playerPriority = playerSkill.SkillBase.Priority;
//         int enemyPriority = enemySkill.SkillBase.Priority;

//         bool playerGoesFirst;

//         if (enemyPriority > playerPriority)
//         {
//             playerGoesFirst = false;
//         }
//         else if (playerPriority > enemyPriority)
//         {
//             playerGoesFirst = true;
//         }
//         else
//         {
//             // 우선도가 같을 때: 스피드 비교
//             if (player.BattlePokemon.Speed == enemy.BattlePokemon.Speed)
//             {
//                 // 속도가 같으면 랜덤
//                 playerGoesFirst = UnityEngine.Random.Range(0, 2) == 0;
//             }
//             else
//             {
//                 playerGoesFirst = player.BattlePokemon.Speed > enemy.BattlePokemon.Speed;
//             }
//         }

//         return playerGoesFirst ? (player, enemy) : (enemy, player);
//     }
//     public async UniTask HandleNonSkillAction(BattleAction playerAction)
//     {
//         if (playerAction == BattleAction.SwitchPokemon)
//         {
//             var selectedPokemon = playerParty.Party[currentMember];
//             state = BattleState.Busy;
//             await SwitchPokemon(selectedPokemon);
//         }
//         else if (playerAction == BattleAction.UseItem)
//         {
//             dialogBox.EnableActionSelector(false);
//             await ThrowPokeball();
//         }
//         else if (playerAction == BattleAction.Run)
//         {
//             await TryToRun();
//         }
//     }
//     public async UniTask RunSkill(BattleUnit sourceUnit, BattleUnit targetUnit, Skill skill)
//     {
//         bool canRunSkill = sourceUnit.BattlePokemon.OnBeforeSkill();
//         if (!canRunSkill)
//         {
//             await ShowStatusChanges(sourceUnit.BattlePokemon);
//             await sourceUnit.BattleHud.UpdateHp();
//             return;
//         }

//         await ShowStatusChanges(sourceUnit.BattlePokemon);

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

//         // PP 감소
//         skill.PP--;

//         await dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.P_Base.PokemonName}의 {skill.SkillBase.SkillName}!");

//         // 명중 여부
//         if (CheckSkillHits(skill, sourceUnit.BattlePokemon, targetUnit.BattlePokemon))
//         {
//             if (skill.SkillBase.CategoryKey == CategoryKey.Status)
//             {
//                 if (targetUnit.BattlePokemon.Status != null)
//                 {
//                     await dialogBox.TypeDialog("효과가 없는 것 같다...");
//                 }
//                 else
//                 {
//                     await RunSkillEffects(skill.SkillBase.Effects, sourceUnit.BattlePokemon, targetUnit.BattlePokemon, skill.SkillBase.Target);
//                 }
//             }
//             else
//             {
//                 var (startHp, endHp, damageDetails) = targetUnit.BattlePokemon.TakeDamage(skill, sourceUnit.BattlePokemon);
//                 await targetUnit.BattleHud.UpdateHp();
//                 await ShowDamageDetails(damageDetails);
//             }

//             // 부가 효과 처리
//             if (skill.SkillBase.SecondaryEffects != null &&
//                 skill.SkillBase.SecondaryEffects.Count > 0 &&
//                 targetUnit.BattlePokemon.PokemonHp > 0)
//             {
//                 foreach (var secondary in skill.SkillBase.SecondaryEffects)
//                 {
//                     var rnd = UnityEngine.Random.Range(1, 101);
//                     if (rnd <= secondary.Chance)
//                     {
//                         await RunSkillEffects(secondary, sourceUnit.BattlePokemon, targetUnit.BattlePokemon, secondary.Target);
//                     }
//                 }
//             }

//             if (targetUnit.BattlePokemon.PokemonHp <= 0)
//             {
//                 await HandlePokemonFainted(targetUnit);
//             }
//         }
//         else
//         {
//             if (sourceUnit.IsPlayerUnit)
//             {
//                 await dialogBox.TypeDialog($"상대 {targetUnit.BattlePokemon.P_Base.PokemonName}에게는 \n맞지 않았다!");
//             }
//             else
//             {
//                 await dialogBox.TypeDialog($"{targetUnit.BattlePokemon.P_Base.PokemonName}에게는 맞지 않았다!");
//             }
//         }
//     }

//     public async UniTask RunAfterTurn(BattleUnit sourceUnit)
//     {
//         if (state == BattleState.BattleOver)
//             return;

//         // 상태가 RunningTurn이 될 때까지 대기 (프레임단위 대기 → UniTask 대기)
//         await UniTask.WaitUntil(() => state == BattleState.RunningTurn);

//         await UniTask.Delay(TimeSpan.FromSeconds(1.2f));

//         // 상태이상 처리
//         sourceUnit.BattlePokemon.OnAfterTurn();
//         await ShowStatusChanges(sourceUnit.BattlePokemon);
//         await sourceUnit.BattleHud.UpdateHp();

//         if (sourceUnit.BattlePokemon.PokemonHp <= 0)
//         {
//             await HandlePokemonFainted(sourceUnit);
//             await UniTask.WaitUntil(() => state == BattleState.RunningTurn);
//         }
//     }
//     public async UniTask RunSkillEffects(SkillEffects effects, Pokemon sourceUnit, Pokemon targetUnit, SkillTarget skillTarget)
//     {
//         // 랭크업
//         if (effects.Rankup != null)
//         {
//             if (skillTarget == SkillTarget.Self)
//                 sourceUnit.ApplyRankups(effects.Rankup);
//             else
//                 targetUnit.ApplyRankups(effects.Rankup);
//         }

//         // 상태이상
//         if (effects.Status != ConditionID.None)
//         {
//             targetUnit.SetStatus(effects.Status);
//         }

//         // 일시적 상태이상
//         if (effects.VolatileStatus != ConditionID.None)
//         {
//             targetUnit.SetVolatileStatus(effects.VolatileStatus);
//         }

//         await ShowStatusChanges(sourceUnit);
//         await ShowStatusChanges(targetUnit);
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
//     public async UniTask ShowStatusChanges(Pokemon pokemon)
//     {
//         while (pokemon.StatusCngMsg.Count > 0)
//         {
//             string message = pokemon.StatusCngMsg.Dequeue();
//             await dialogBox.TypeDialog(message);
//         }
//     }
//     public async UniTask HandlePokemonFainted(BattleUnit faintedUnit)
//     {
//         var faintedName = faintedUnit.BattlePokemon.P_Base.PokemonName;
//         await dialogBox.TypeDialog($"{faintedName}{GetCorrectParticle(faintedName, "topic")} 쓰러졌다!");

//         await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

//         if (!faintedUnit.IsPlayerUnit)
//         {
//             int expYield = faintedUnit.BattlePokemon.P_Base.ExpYield;
//             int enemyLevel = faintedUnit.BattlePokemon.PokemonLevel;
//             float trainerBonus = isTrainerBattle ? 1.5f : 1.0f;

//             int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus / 7);
//             playerUnit.BattlePokemon.PokemonExp += expGain;

//             await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")}\n{expGain} 경험치를 얻었다!");
//             await playerUnit.BattleHud.SetExpSmooth();
//             await CheckLearnableSkill();
//         }

//         await CheckForBattleOver(faintedUnit);

//         GameManager.Inst.AddGold();
//     }

//     bool cancelSelected = false;
//     public async UniTask CheckLearnableSkill()
//     {
//         while (playerUnit.BattlePokemon.CheckForLevelUp())
//         {
//             playerUnit.BattleHud.SetLevel();
//             await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 레벨 {playerUnit.BattlePokemon.PokemonLevel}로 올랐다!");

//             var newSkill = playerUnit.BattlePokemon.GetLearnableSkill();
//             if (newSkill != null)
//             {
//                 if (playerUnit.BattlePokemon.Skills.Count < PokemonBase.MaxNumOfSkills)
//                 {
//                     playerUnit.BattlePokemon.LearnSkill(newSkill);
//                     await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 새로 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배웠다!");
//                     dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);
//                 }
//                 else
//                 {
//                     bool isFinalDecisionMade = false;
//                     while (!isFinalDecisionMade)
//                     {
//                         await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 새로 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우고 싶어한다!");
//                         await dialogBox.TypeDialog($"하지만 기술이 4개이므로 다른 기술을 잊어야 한다.");
//                         await dialogBox.TypeDialog($"{newSkill.SkillBase.SkillName} 대신 다른 기술을 잊게 하겠습니까?");

//                         ConfirmBoxSelection();
//                         await UniTask.WaitUntil(() => state != BattleState.ConfirmBox);
//                         bool isConfirmed = HandleConfirmBoxSelection();

//                         if (isConfirmed)
//                         {
//                             await ChooseSkillToForget(playerUnit.BattlePokemon, newSkill.SkillBase);
//                             await UniTask.WaitUntil(() => state == BattleState.Busy);

//                             if (cancelSelected || currentSelection == PokemonBase.MaxNumOfSkills)
//                             {
//                                 cancelSelected = false;

//                                 await dialogBox.TypeDialog($"그럼... {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우는 것을 포기하겠습니까?");
//                                 ConfirmBoxSelection();
//                                 await UniTask.WaitUntil(() => state != BattleState.ConfirmBox);
//                                 bool giveUp = HandleConfirmBoxSelection();

//                                 if (giveUp)
//                                 {
//                                     await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 결국 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우지 않았다!");
//                                     skillToLearn = null;
//                                     isFinalDecisionMade = true;
//                                 }
//                                 else
//                                 {
//                                     await ChooseSkillToForget(playerUnit.BattlePokemon, newSkill.SkillBase);
//                                     await UniTask.WaitUntil(() => state == BattleState.SkillToForget);
//                                 }
//                             }
//                             else
//                             {
//                                 var oldSkill = playerUnit.BattlePokemon.Skills[currentSelection].SkillBase;
//                                 playerUnit.BattlePokemon.Skills[currentSelection] = new Skill(newSkill.SkillBase);

//                                 await dialogBox.TypeDialog("1, 2, ... 짠!");
//                                 await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} {oldSkill.SkillName}{GetCorrectParticle(oldSkill.SkillName, "object")} 깨끗이 잊었다!");
//                                 await dialogBox.TypeDialog($"그리고 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배웠다!");

//                                 dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);
//                                 skillToLearn = null;
//                                 isFinalDecisionMade = true;
//                             }
//                         }
//                         else
//                         {
//                             await dialogBox.TypeDialog($"그럼... {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우는 것을 포기하겠습니까?");
//                             ConfirmBoxSelection();
//                             await UniTask.WaitUntil(() => state != BattleState.ConfirmBox);
//                             bool isReallyConfirmed = HandleConfirmBoxSelection();

//                             if (isReallyConfirmed)
//                             {
//                                 await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 결국 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우지 않았다!");
//                                 skillToLearn = null;
//                                 isFinalDecisionMade = true;
//                             }
//                         }
//                     }
//                 }
//             }

//             await playerUnit.BattleHud.SetExpSmooth(true);
//         }
//     }

//     public async UniTask CheckForBattleOver(BattleUnit faintedUnit)
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
//         else // 적 유닛이 쓰러졌을 때
//         {
//             if (!isTrainerBattle)
//             {
//                 await playerParty.CheckForEvolutions(); // 진화 로직도 UniTask 기반이어야 함
//                 await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
//                 BattleOver(true);
//             }
//             else
//             {
//                 var nextPokemon = trainerParty.GetHealthyPokemon();
//                 if (nextPokemon != null)
//                 {
//                     await playerParty.CheckForEvolutions();
//                     // 이후 상대 포켓몬 교체 처리 (선택적으로 확장 가능)
//                 }
//                 else
//                 {
//                     await playerParty.CheckForEvolutions();
//                     BattleOver(true);
//                 }
//             }
//         }
//     }

//     public async UniTask ShowDamageDetails(DamageDetails damageDetails)
//     {
//         if (damageDetails.Critical > 1f)
//         {
//             await dialogBox.TypeDialog("급소에 맞았다!");
//         }

//         if (damageDetails.TypeEffectiveness > 1f)
//         {
//             if (damageDetails.TypeEffectiveness > 2f)
//             {
//                 await dialogBox.TypeDialog("효과가 굉장했다!!");
//             }
//             else
//             {
//                 await dialogBox.TypeDialog("효과가 대단했다!");
//             }
//         }
//         else if (damageDetails.TypeEffectiveness < 1f)
//         {
//             if (damageDetails.TypeEffectiveness == 0f)
//             {
//                 await dialogBox.TypeDialog("효과가 없는 듯 하다...");
//             }
//             else
//             {
//                 await dialogBox.TypeDialog("효과가 별로인듯 하다...");
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
//                 UniTask.Void(async () =>
//            {
//                await RunTurns(BattleAction.UseItem);
//            });
//             }
//             else if (currentAction == 2)
//             {//포켓몬
//                 preState = state;
//                 OpenPartyScreen();
//             }
//             else if (currentAction == 3)
//             {//도망친다 
//                 UniTask.Void(async () =>
//                     {
//                         await RunTurns(BattleAction.Run);
//                     });
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
//                 UniTask.Void(async () =>
//                 {
//                     await dialogBox.TypeDialog("기술의 남은 포인트가 없다!");
//                 });
//                 ActionSelection();
//                 return;
//             }

//             dialogBox.EnableSkillSelector(false);
//             dialogBox.EnableDialogText(true);
//             UniTask.Void(async () =>
//         {
//             await RunTurns(BattleAction.Skill);
//         });
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
//         if (preState == BattleState.ActionSelection && Input.GetKeyDown(KeyCode.Backspace))
//         {
//             partyScreen.gameObject.SetActive(false);
//             ActionSelection();
//             return;
//         }
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
//             Debug.Log("[PartyScreen] Space/Enter 입력 감지됨");
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
//                 Debug.Log("[PartyScreen] 전술적 교체 → RunTurns 호출");
//                 preState = null;
//                 UniTask.Void(async () =>
//                 {
//                     await RunTurns(BattleAction.SwitchPokemon);
//                 });
//                 dialogBox.EnableActionSelector(false);


//             }
//             //포켓몬이 쓰러졌을때
//             else
//             {
//                 Debug.Log("[PartyScreen] 강제 교체 → SwitchPokemon 호출");
//                 state = BattleState.Busy;

//                 UniTask.Void(async () =>
//                 {
//                     await SwitchPokemon(selectedMember);
//                 });
//             }

//             Debug.Log($"[PartySelect] 선택된 포켓몬: {selectedMember.P_Base.PokemonName}, 상태: {state}, preState: {preState}");
//         }
//     }
//     public async UniTask SwitchPokemon(Pokemon newPokemon)
//     {
//         playerUnit.BattlePokemon.CureVolatileStatus();
//         playerUnit.BattlePokemon.ResetRankup();

//         await dialogBox.TypeDialog($"돌아와 {playerUnit.BattlePokemon.P_Base.PokemonName}!");

//         // TODO: 사망 애니메이션 들어갈 부분
//         await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

//         // 현재 전투 중인 포켓몬 (인덱스 0)
//         var currentBattlePokemon = playerParty.Party[0];

//         // 포켓몬 교체 (자리 바꾸기)
//         playerParty.Party[0] = newPokemon;
//         playerParty.Party[currentMember] = currentBattlePokemon;

//         playerUnit.SetUp(newPokemon);
//         dialogBox.SetSkillNames(newPokemon.Skills);
//         skillCount = newPokemon.Skills.Count;

//         await dialogBox.TypeDialog($"가랏! {newPokemon.P_Base.PokemonName}!");

//         state = BattleState.RunningTurn;
//         Debug.Log($"[SwitchPokemon] 교체: {newPokemon.P_Base.PokemonName}");

//     }

//     #endregion
//     #region LearnSkill
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
//     public async UniTask ThrowPokeball()
//     {
//         state = BattleState.Busy;

//         if (isTrainerBattle)
//         {
//             await dialogBox.TypeDialog("다른 트레이너의 포켓몬은 잡을 수 없다!");
//             state = BattleState.RunningTurn;
//             return;
//         }

//         var pokeballObj = Instantiate(Pokeball, playerUnit.transform.position, Quaternion.identity);
//         var pokeball = pokeballObj.GetComponent<SpriteRenderer>();

//         // TODO: pokeball.transform.DoMove 등의 애니메이션 처리

//         int shakeCount = TryToCatchPokemon(enemyUnit.BattlePokemon);

//         for (int i = 0; i < Math.Min(shakeCount, 3); ++i)
//         {
//             await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
//             // TODO: 흔들림 애니메이션 처리
//         }

//         if (shakeCount == 4)
//         {
//             playerParty.AddPokemon(enemyUnit.BattlePokemon);
//             GlobalValue.CatchPokemon(enemyUnit.BattlePokemon.P_Base, false);
//             UnityEngine.Object.Destroy(pokeball);

//             await dialogBox.TypeDialog($"신난다-!\n야생 {enemyUnit.BattlePokemon.P_Base.PokemonName}을 잡았다!");
//             BattleOver(true);
//         }
//         else
//         {
//             await dialogBox.TypeDialog($"!");
//             UnityEngine.Object.Destroy(pokeball);
//             state = BattleState.RunningTurn;
//         }

//         await UniTask.Delay(TimeSpan.FromSeconds(1.0f));
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

//     public async UniTask TryToRun()
//     {
//         state = BattleState.Busy;

//         if (isTrainerBattle)
//         {
//             await dialogBox.TypeDialog("상대 트레이너와의 배틀에서는 도망칠 수 없다!");
//             state = BattleState.RunningTurn;
//             return;
//         }

//         escapeAttempts++;

//         int playerSpeed = playerUnit.BattlePokemon.Speed;
//         int enemySpeed = enemyUnit.BattlePokemon.Speed;

//         if (playerSpeed >= enemySpeed)
//         {
//             await dialogBox.TypeDialog("무사히 도망쳤다!");
//             BattleOver(true);
//             return;
//         }

//         float f = (playerSpeed * 128f) / (enemySpeed + 30f * escapeAttempts);
//         f = f % 256;

//         if (UnityEngine.Random.Range(0, 256) < f)
//         {
//             await dialogBox.TypeDialog("무사히 도망쳤다!");
//             BattleOver(true);
//         }
//         else
//         {
//             await dialogBox.TypeDialog("도망칠 수 없었다!");
//             state = BattleState.RunningTurn;
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
