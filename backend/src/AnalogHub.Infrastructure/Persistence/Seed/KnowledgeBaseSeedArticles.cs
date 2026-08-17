using AnalogHub.Domain.Enums;

namespace AnalogHub.Infrastructure.Persistence.Seed;

internal sealed record SeedArticle(string Title, KnowledgeSourceType SourceType, string? SourceUrl, string Content);

/// <summary>Practical reference articles ingested at seed time so the RAG assistant has something real to answer from.</summary>
internal static class KnowledgeBaseSeedArticles
{
    public static readonly IReadOnlyList<SeedArticle> All =
    [
        new SeedArticle(
            "Portrait Composition Fundamentals",
            KnowledgeSourceType.Technique,
            null,
            """
            Good portrait composition starts before you raise the camera to your eye. The frame is a
            small stage, and every element in it either supports the subject or competes with them.

            Rule of thirds and eye placement. Place the subject's eyes along the upper third line of
            the frame rather than dead center. Eyes are where a viewer's gaze lands first, and putting
            them on a power point gives the composition immediate tension and life. For a classic
            three-quarter portrait, position the subject slightly off-center and leave more headroom
            on the side they are looking toward — this "looking room" keeps the frame from feeling
            cramped or claustrophobic.

            Framing distance and lens choice. A 50mm lens on 35mm film renders perspective close to
            how the eye sees, which makes it a safe default for environmental portraits that include
            context. For flattering facial proportions, an 85mm-to-135mm equivalent compresses
            features slightly and avoids the nose-enlarging distortion you get from getting too close
            with a wide lens. On medium format, a 75mm-to-110mm lens on 645 plays the same role as an
            85mm-135mm on 35mm.

            Background separation. Shoot at wide apertures (f/2 to f/2.8) to throw the background out
            of focus and pull the viewer's attention to the subject's face. If your lens's maximum
            aperture is modest, increase the physical distance between subject and background instead
            — doubling that distance has a bigger effect on background blur than most people expect,
            and costs nothing.

            Negative space and the rule of odds. Empty space is not wasted space; it gives the subject
            room to exist in the frame and can communicate mood — a large sky above a small figure
            reads as solitude or scale. When photographing groups, odd numbers (three, five) tend to
            compose more naturally than even numbers, which the eye tries to pair off symmetrically.

            Leading lines and framing elements. Doorways, fences, tree branches, and architectural
            lines can direct the eye toward the subject or literally frame them within the frame. Scan
            the edges of your viewfinder before shooting — a stray branch or bright highlight at the
            border pulls attention away from the face just as much as a distracting background would.

            Common mistakes to avoid: centering every subject out of habit, cutting off hands or feet
            awkwardly at the frame edge, and shooting from below eye level for adult subjects (which
            can look unflattering unless done deliberately for effect). Get down to the subject's eye
            level, or slightly above, for the most natural-feeling portrait.
            """),
        new SeedArticle(
            "Manual Flash Technique for Film Photography",
            KnowledgeSourceType.Technique,
            null,
            """
            Film cameras rarely have the sophisticated TTL flash metering of modern digital bodies, so
            understanding manual flash exposure is essential for consistent results.

            Guide numbers and the inverse-square law. A flash's guide number (GN) tells you how much
            light it can throw at ISO 100. The core manual-flash formula is: Aperture = Guide Number ÷
            Distance (in meters). A flash with GN 36 lighting a subject 4 meters away needs f/9
            (36 ÷ 4 = 9). Move the subject to 2 meters and the correct aperture jumps to f/18 — light
            falls off with the square of distance, which is why small changes in flash-to-subject
            distance matter far more than small changes in flash power.

            Sync speed. Focal-plane shutters (in nearly all 35mm SLRs) have a maximum flash sync speed,
            typically 1/125s to 1/250s. Above that speed, the shutter curtain is still traveling across
            the frame when the flash fires, and part of the image will be cut off by a dark band. Leaf
            shutters, common on medium format cameras like the Pentax 645 or a Hasselblad, can sync at
            any speed, which is a real advantage for balancing flash with bright ambient light.

            Fill flash vs. key flash. Used as fill, flash output should sit one to two stops below the
            ambient exposure so it lifts shadow detail without looking obviously "flashed" — useful for
            backlit portraits or harsh midday sun. Used as the key (primary) light, flash output should
            match or exceed ambient so it becomes the dominant light source, letting you underexpose a
            bright background for a more dramatic, moody look.

            Bounce and diffusion. Direct, on-camera flash produces flat light and harsh shadows.
            Bouncing the flash head off a white ceiling or wall softens the light dramatically and
            adds a more natural falloff, though it costs you one to two stops of effective power and
            shifts color slightly if the bounce surface isn't neutral white. A simple diffuser dome or
            even a folded white card ("bounce card") angled off the flash head is a cheap way to soften
            direct flash when there's no ceiling to bounce from.

            Working manually without a meter. If you don't have a flash meter, take a test shot (or a
            digital reference shot at matching ISO/aperture) at your calculated aperture, check the
            result, and adjust the aperture by full stops until skin tones look correct — err
            slightly underexposed with color negative film, which has generous highlight latitude.
            """),
        new SeedArticle(
            "35mm vs Medium Format (120): Choosing Your Film Format",
            KnowledgeSourceType.Guide,
            null,
            """
            The choice between 35mm and 120 (medium format) roll film shapes almost every practical
            aspect of a shoot, from camera size to how many frames you get before reloading.

            Image quality and grain. A 120 negative in 6x6 or 6x7 format has roughly three to four
            times the surface area of a 35mm frame. More surface area means finer apparent grain at
            any given enlargement size and noticeably more resolved detail — a 120 negative can often
            be printed twice as large as a 35mm negative from the same film stock before grain becomes
            objectionable. If large prints or maximum detail are the goal, medium format has a real
            technical edge.

            Depth of field and rendering. For an equivalent framing, medium format lenses use longer
            focal lengths than their 35mm counterparts (a 75mm on 645 frames like a 45-50mm on 35mm),
            which combined with the larger negative gives a shallower depth of field and a distinctive
            "medium format look" — creamier out-of-focus rendering that's prized for portrait work.

            Frame count and cost per shot. A roll of 35mm typically gives 24 or 36 exposures; a roll of
            120 gives anywhere from 8 (6x9) to 16 (645) frames depending on the camera's frame size.
            Medium format shooters develop a more deliberate, slower shooting rhythm out of necessity —
            each frame costs noticeably more in film and processing, which naturally encourages more
            careful composition and fewer "spray and pray" frames.

            Camera size, weight and handling. 35mm SLRs and rangefinders are compact and fast to
            handle, well suited to street photography, travel and any situation demanding
            spontaneity. Medium format bodies are larger, heavier, and often slower to operate — manual
            focus, no motor drive on many bodies, and a waist-level finder that shows a mirrored image
            on TLRs and some SLRs. That slower pace is a feature for planned portrait or landscape
            work, and a liability for fast-moving documentary or street shooting.

            Which to choose. Pick 35mm for volume, speed and portability — travel, street, events,
            anything where you need many frames and fast handling. Pick 120 for deliberate,
            high-quality work where negative quality and rendering matter more than frame count —
            studio and location portraits, landscapes, and fine-art prints. Many photographers
            eventually run both: a 35mm body for everyday carry and a medium format body reserved for
            planned shoots.
            """)
    ];
}
