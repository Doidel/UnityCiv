 using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CharacterMovement : MonoBehaviour
{
    private float _maxpoints = 0;
    public float MovementPointsMax
    {
        get
        {
            return _maxpoints;
        }
        set
        {
            _maxpoints = value;
            MovementPointsRemaining = (int)value;
        }
    }

    private int _baseSightRadius = 1;
    public int SightRadius {
        get {
            if (curTile.Building != null && curTile.Building is Outpost)
            {
                return Outpost.SightRadius;
            }
            return _baseSightRadius;
        }
        set
        {
            _baseSightRadius = value;
        }
    }

    public int MovementPointsRemaining
    {
        get;
        private set;
    }

    #region audio clips
    public string AttackAudioClipPath;
    public string BeatenAudioClipPath;
    public string DieAudioClipPath;
    private AudioClip _attackAudioClip;
    public AudioClip AttackAudioClip
    {
        get
        {
            if (_attackAudioClip == null && !string.IsNullOrEmpty(AttackAudioClipPath))
                _attackAudioClip = Resources.Load<AudioClip>(AttackAudioClipPath);
            return _attackAudioClip;
        }
    }
    private AudioClip _beatenAudioClip;
    public AudioClip BeatenAudioClip
    {
        get
        {
            if (_beatenAudioClip == null && !string.IsNullOrEmpty(BeatenAudioClipPath))
                _beatenAudioClip = Resources.Load<AudioClip>(BeatenAudioClipPath);
            return _beatenAudioClip;
        }
    }
    private AudioClip _dieAudioClip;
    public AudioClip DieAudioClip
    {
        get
        {
            if (_dieAudioClip == null && !string.IsNullOrEmpty(DieAudioClipPath))
                _dieAudioClip = Resources.Load<AudioClip>(DieAudioClipPath);
            return _dieAudioClip;
        }
    }
    #endregion

    public string AnimIdle = "idle";
    public string AnimWalk = "walk";
    public string AnimAttack = "attack1";
    public string AnimBeaten = "beaten1";
    public string AnimDie = "die1";
    [HideInInspector]
    public float MovementSpeed = 1f;

    public CombatType Type = CombatType.NoCombat;
    public int MaxHealth = 1;
    public int Health = 1;
    public float Strength = 0;

    public bool ExpendMovementPoints(int points)
    {
        if (!IsMoving && MovementPointsRemaining >= points)
        {
            MovementPointsRemaining -= points;
            return true;
        }
        return false;
    }

    //speed in meters per second
    private float baseSpeed = 0.025f;
    //distance between character and tile position when we assume we reached it and start looking for the next. Explained in detail later on
    public const float MinNextTileDist = 0.0015f;
    //distance between current and next tile where the attack should start
    public const float AttackDistSquared = 0.5f * 0.5f;
    //position of the tile we are heading to
    public Vector3 curTilePos
    {
        get;
        private set;
    }
    public Tile curTile
    {
        get;
        private set;
    }
    private List<Tile> path;
    private IGameUnit enemyToAttack = null;
    private bool rotateAfterAttack = false;
    public bool IsMoving { get; private set; }

    // which animations are running?
    public float AttackRunning { get; private set; }
    public float BeatenRunning { get; private set; }
    public float DeathRunning { get; private set; }

    public List<Tile> RemainingPath
    {
        get;
        private set;
    }

    private Animator anim;
    
    private PopupText popupTextPrefab;
    private Healthbar healthbar;
    private UnitNumbersDisplay unitNumbersDisplay;

    void Awake()
    {
        IsMoving = false;
        popupTextPrefab = Resources.Load<PopupText>("Prefabs/PopupTextParent");
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        var canvas = GetComponent<IGameUnit>().UnitCanvas.transform;
        healthbar = Instantiate(Resources.Load<Healthbar>("Prefabs/Healthbar"));
        healthbar.transform.SetParent(canvas, false);
        unitNumbersDisplay = Instantiate(Resources.Load<UnitNumbersDisplay>("Prefabs/UnitNumbersDisplay"));
        unitNumbersDisplay.transform.SetParent(canvas, false);
        unitNumbersDisplay.SetUnits(GridManager.instance.GetFriendlyUnitsOnTile(GetComponent<IGameUnit>().OwnedBy, curTile));
        //GetAttacked(1, true);
        //AdjustPlayerSight(curTile);
    }

    public Tile setPos(Vector2 pos)
    {
        Tile cTile;
        if (GridManager.instance.board.TryGetValue(new Point((int)pos.x - (int)pos.y/2, (int)pos.y), out cTile))
        {
            setPos(cTile);
        }
        else
        {
            throw new UnityException("No valid start pos!");
        }

        return cTile;
    }

    public void setPos(Tile tile)
    {
        curTile = tile;
        curTilePos = GridManager.instance.calcWorldCoord(new Vector2(tile.X, tile.Y));
        gameObject.transform.position = curTilePos;
        RemainingPath = new List<Tile>() { curTile };

        // remove fog of next tile and surrounding tiles, if any
        if (GetComponent<IGameUnit>().OwnedBy == GameManager.instance.LocalPlayer)
            GridManager.instance.UncoverTiles(GridManager.instance.GetHexArea(curTile, SightRadius));

        AdjustPlayerSight(curTile);
    }

    public bool MoveTo(Vector2 dest)
    {
        if (!IsMoving)
        {
            var GM = GridManager.instance;
            var pathlist = new List<Tile>(GM.generatePath(new Vector2(curTile.X, curTile.Y), dest));
            pathlist.Reverse();

            // TODO: Assuming 1 tile = 1 movement
            RemainingPath = pathlist;
            StartMoving(RemainingPath.Count - 1 <= MovementPointsRemaining);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Starts moving along the RemainingPath.
    /// </summary>
    /// <param name="potentialAttack">If true, then the unit will attack if the last tile is an enemy.</param>
    void StartMoving(bool potentialAttack = false)
    {
        if (MovementPointsRemaining > 0)
        {
            var movement = Math.Min(MovementPointsRemaining, RemainingPath.Count - 1);
            var path = RemainingPath.Take(movement + 1).ToList();
            if (path.Count <= 1)
                return;
            RemainingPath = RemainingPath.Skip(movement).ToList();
            ExpendMovementPoints(movement);

            path.Reverse();

            TimeManager.instance.PerformingAction();
            TimeManager.instance.NoMoreOrdersNeeded(gameObject.GetComponent<AbstractCommandableEntity>());

            //the first tile we need to reach is actually in the end of the list just before the one the character is currently on
            curTile = path[path.Count - 2];
            curTilePos = getWorld(curTile);

            // remove fog of next tile and surrounding tiles, if any
            if (GetComponent<IGameUnit>().OwnedBy == GameManager.instance.LocalPlayer)
                GridManager.instance.UncoverTiles(GridManager.instance.GetHexArea(curTile, SightRadius));

            AdjustPlayerSight(curTile);

            // Rotate towards the target
            transform.rotation = Quaternion.LookRotation(curTilePos - transform.position);

            IsMoving = true;
            this.path = path;
            enemyToAttack = null;
            if (potentialAttack)
                enemyToAttack = GridManager.instance.GetEnemiesOnTile(GetComponent<IGameUnit>().OwnedBy, this.path[0]).FirstOrDefault();
        }
    }

    void Update()
    {
        var remainingDistSquared = (curTilePos - transform.position).sqrMagnitude;


        // if the last tile should be attacked and we're about to reach the last tile
        if (AttackRunning > 0)
        {
            AttackRunning -= Time.deltaTime;
            anim.Play(AnimAttack, -1);
            // if the animation is finished
            if (AttackRunning <= 0f)
            {
                AttackRunning = 0f;
                // redirect the path to return to the secondlast position.
                path = path.Skip(1).ToList();
                curTile = path[0];
                curTilePos = getWorld(curTile);
                transform.rotation = Quaternion.LookRotation(curTilePos - transform.position);
            }
        }
        else if (enemyToAttack != null && path.IndexOf(curTile) == 0 && remainingDistSquared < AttackDistSquared)
        {
            AttackRunning = 0.8f; // run attack anim for AttackRunning seconds
            Attack(enemyToAttack);
            enemyToAttack = null;
            rotateAfterAttack = true;
        }
        else if (DeathRunning > 0)
        {
            DeathRunning -= Time.deltaTime;
            anim.Play(AnimDie, -1);
        }
        else if (BeatenRunning > 0)
        {
            BeatenRunning -= Time.deltaTime;
            anim.Play(AnimBeaten, -1);
            if (BeatenRunning <= 0f)
                anim.Play(AnimIdle, -1);
        }
        else
        {
            if (!IsMoving)
                return;

            //if the distance between the character and the center of the next tile is short enough
            if (remainingDistSquared < MinNextTileDist * MinNextTileDist)
            {
                // Remove the way marker for the reached tile
                RemoveNextWayMarker();
                
                unitNumbersDisplay.SetUnits(GridManager.instance.GetFriendlyUnitsOnTile(GetComponent<IGameUnit>().OwnedBy, curTile));

                //if we reached the destination tile
                if (path.IndexOf(curTile) == 0)
                {
                    IsMoving = false;
                    anim.Play(AnimIdle, -1);
                    if (rotateAfterAttack)
                    {
                        transform.Rotate(new Vector3(0, 180, 0));
                        //transform.rotation = Quaternion.LookRotation(getWorld(path[1]) - transform.position);
                        rotateAfterAttack = false;
                    }
                    TimeManager.instance.FinishedAction();
                    return;
                }

                //curTile becomes the next one
                curTile = path[path.IndexOf(curTile) - 1];
                curTilePos = getWorld(curTile);

                // remove fog of next tile and surrounding tiles, if any
                if (GetComponent<IGameUnit>().OwnedBy == GameManager.instance.LocalPlayer)
                    GridManager.instance.UncoverTiles(GridManager.instance.GetHexArea(curTile, SightRadius));

                AdjustPlayerSight(curTile);

                // Rotate towards the target
                transform.rotation = Quaternion.LookRotation(curTilePos - transform.position);
            }
            MoveTowards(curTilePos);
        }
    }

    void MoveTowards(Vector3 position)
    {
        //mevement direction
        Vector3 dir = position - transform.position;
        var movement = MovementSpeed * baseSpeed * Time.deltaTime * 40;
        if (dir.sqrMagnitude > movement * movement)
        {
            dir.Normalize();
            dir *= movement;
        }

        //float commonGround = Vector3.Dot(dir.normalized, transform.forward);

        /*Vector3 forwardDir = transform.forward;
        forwardDir = forwardDir * speed;
        float speedModifier = Vector3.Dot(dir.normalized, transform.forward);
        forwardDir *= speedModifier;*/

        //var idlehash = Animator.StringToHash("idle");
        //Vector3 forwardDir = transform.forward;
        //forwardDir = forwardDir * speed;

        //var positionstr = position.ToString("F5");
        //var transformdir = transform.position.ToString("F5");
        //var dirstr = dir.ToString("F5");

        if (dir.sqrMagnitude > MinNextTileDist * MinNextTileDist)
        {
            transform.Translate(dir, Space.World);
            //controller.SimpleMove(forwardDir);
            anim.Play(AnimWalk, -1);
        }
        else {
            anim.Play(AnimIdle, -1);
        }
    }

    private List<Tile> lastTiles = new List<Tile>();
    private void AdjustPlayerSight(Tile curTile)
    {
        if (GetComponent<IGameEntity>().OwnedBy == GameManager.instance.LocalPlayer)
        {
            foreach (var t in lastTiles)
                t.EntityViewCount--;
            var sightArea = GridManager.instance.GetHexArea(curTile, SightRadius);
            foreach (var t in sightArea)
                t.EntityViewCount++;
            lastTiles = sightArea;
        }
        else
        {
            // the other counterpart of setting layers is in Tile.EntityViewCount
            var layer = curTile.EntityViewCount > 0 ? 0 : 12;
            foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = layer;
        }
    }

    private void Attack(IGameUnit enemy)
    {
        var enemyMovement = enemy.gameObject.GetComponent<CharacterMovement>();
        enemyMovement.GetAttacked((int)Strength, true);
        // retaliation. Happens only if both are melee and the enemy has attack and is not dead
        if (enemyMovement.Type == CombatType.Melee && Type == CombatType.Melee && enemyMovement.Strength > 0 && enemyMovement.Health > 0)
        {
            // rotate towards attacker
            enemyMovement.transform.rotation = Quaternion.LookRotation(transform.position - enemyMovement.transform.position);
            GetAttacked((int)enemyMovement.Strength, false);
        }

        if (AttackAudioClip != null)
            GetComponent<IGameUnit>().audioSource.PlayOneShot(AttackAudioClip, 0.3f);
    }

    public void GetAttacked(int enemyStrength, bool playAnimation)
    {
        var popupText = Instantiate(popupTextPrefab);
        popupText.transform.SetParent(GameObject.Find("MainCanvas").transform, false);
        popupText.SetText(enemyStrength.ToString());
        popupText.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        Health = Math.Max(0, Health - enemyStrength);
        healthbar.Set(Health, MaxHealth);

        if (Health > 0)
        {
            if (playAnimation)
            {
                BeatenRunning = 0.8f;
                if (BeatenAudioClip != null)
                    GetComponent<IGameUnit>().audioSource.PlayOneShot(BeatenAudioClip, 0.3f);
            }
        }
        else
        {
            GetComponent<IGameUnit>().RemoveCharacter(2f);
            if (playAnimation)
            {
                // play a death animation and remove the gameobject
                DeathRunning = 1.2f;
                if (DieAudioClip != null)
                    GetComponent<IGameUnit>().audioSource.PlayOneShot(DieAudioClip, 0.3f);
            }
        }
    }

    private Vector3 getWorld(Tile t)
    {
        return GridManager.instance.calcWorldCoordWiggly(new Vector2(t.X + (int)Math.Floor(t.Y / 2d), t.Y));
    }

    void OnEnable()
    {
        EventManager.StartListening("NextRound", nextRoundListener);
        EventManager.StartListening("NextRoundRequest", nextRoundRequestListener);
    }

    void OnDisable()
    {
        EventManager.StopListening("NextRound", nextRoundListener);
        EventManager.StopListening("NextRoundRequest", nextRoundRequestListener);
    }

    private void nextRoundRequestListener()
    {
        // perform remaining movement
        if (MovementPointsRemaining > 0 && RemainingPath.Count > 1)
        {
            StartMoving();
        }
    }

    private void nextRoundListener()
    {
        MovementPointsRemaining = (int)MovementPointsMax;
        // draw the next path's reachability for this round if the unit's selected
        if (GridManager.instance.selectedUnit == gameObject)
            DrawPath(RemainingPath);
    }

    private static List<GameObject> suggestedMovePathLineObjects = new List<GameObject>();
    private static Color thisRoundMoveColor = new Color(0.31f, 0.27f, 0.1f, 1);
    private static Color nextRoundsMoveColor = new Color(0.65f, 0.65f, 0.65f, 1);

    internal void SuggestMove(Vector3 dest)
    {
        if (!IsMoving)
        {
            var pathlist = new List<Tile>(GridManager.instance.generatePath(new Vector2(curTile.X, curTile.Y), dest));
            pathlist.Reverse();
            DrawPath(pathlist);
        }
    }

    public void DrawSuggestedPath()
    {
        DrawPath(RemainingPath);
    }

    private void DrawPath(List<Tile> pathlist)
    {
        // starting at 1 because the first tile in the list is the current tile itself
        for (int t = 1; t < pathlist.Count; t++)
        {
            var tile = pathlist[t];
            GameObject point = null;
            if (suggestedMovePathLineObjects.Count > t - 1)
                point = suggestedMovePathLineObjects[t - 1];
            else
            {
                point = Instantiate(GridManager.instance.MovementLineObject);
                suggestedMovePathLineObjects.Add(point);
            }
            point.transform.position = GridManager.instance.calcWorldCoord(new Vector2(tile.X, tile.Y));
            //point.transform.Translate(new Vector3(0f, 0.3f, 0f));
            // TODO: assuming every tile movement costs 1 points
            point.GetComponent<MeshRenderer>().material.color = MovementPointsRemaining - t >= 0 ? thisRoundMoveColor : nextRoundsMoveColor;
            point.SetActive(true);
            // TODO: display a number for each movement point used on tile
        }

        // hide the rest of the (still visible) movement points
        if (suggestedMovePathLineObjects.Count >= pathlist.Count)
        {
            for (int s = Math.Max(pathlist.Count - 1, 0); s < suggestedMovePathLineObjects.Count; s++)
            {
                suggestedMovePathLineObjects[s].SetActive(false);
            }
        }
    }

    public void CancelSuggestedMove()
    {
        RemainingPath = new List<Tile>();
        RemoveWayMarkers();
    }

    private void RemoveNextWayMarker()
    {
        if (GridManager.instance.selectedUnit == gameObject && suggestedMovePathLineObjects.Count > 0 && suggestedMovePathLineObjects[0] != null && suggestedMovePathLineObjects[0].activeSelf)
        {
            var removeWayPoint = suggestedMovePathLineObjects[0];
            removeWayPoint.SetActive(false);
            suggestedMovePathLineObjects = suggestedMovePathLineObjects.Skip(1).ToList();
            suggestedMovePathLineObjects.Add(removeWayPoint);
        }
    }

    /// <summary>
    /// Hides the unit's path
    /// </summary>
    public void RemoveWayMarkers()
    {
        foreach (var o in suggestedMovePathLineObjects)
            if (o != null)
                o.SetActive(false);
    }

    void OnDestroy()
    {
        if (suggestedMovePathLineObjects != null && suggestedMovePathLineObjects.Count > 0)
            RemoveWayMarkers();
        unitNumbersDisplay.SetUnits(GridManager.instance.GetFriendlyUnitsOnTile(GetComponent<IGameUnit>().OwnedBy, curTile));
        if (GetComponent<IGameEntity>().OwnedBy == GameManager.instance.LocalPlayer)
            foreach (var t in lastTiles)
                t.EntityViewCount--;
    }

    public enum CombatType
    {
        NoCombat,
        Melee,
        Ranged
    }
}