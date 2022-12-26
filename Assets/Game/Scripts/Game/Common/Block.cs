using UnityEngine;

public class Block : TileEntity
{
    public BlockType type;
    public Sprite defaultIcon;
    public Sprite firstIcon;
    public Sprite secondIcon;
    public Sprite thirdIcon;

    public override void OnEnable()
    {
        base.OnEnable();
        if (type == BlockType.Empty)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var newColor = spriteRenderer.color;
                newColor.a = 0f;
                spriteRenderer.color = newColor;
            }

            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            transform.localRotation = Quaternion.identity;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (type == BlockType.Empty)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var newColor = spriteRenderer.color;
                newColor.a = 0f;
                spriteRenderer.color = newColor;
            }

            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            transform.localRotation = Quaternion.identity;
        }
    }
}